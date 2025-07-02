using System;
using System.Collections;
using Khaled.MathLib;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NpcAnimationController))]
public class NpcController : MonoBehaviour, IInteractable
{
    private CapsuleCollider _capsuleCollider;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigidbody;
    private NpcAnimationController _animationController;

    private enum ENpcAiState
    {
        Idle = 0,
        FollowPatrolPoints = 1,
        FollowRandomPatrol = 2,
        FollowTarget = 3,
    }

    public enum ENpcMovementStates
    {
        Stationary = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
    }

    [Header("Movement Values")] [SerializeField]
    private NpcMovementValues _npcMovementValues;

    [SerializeField] private ENpcMovementStates _startMoveState = ENpcMovementStates.Running;
    [SerializeField, ReadOnly] private ENpcMovementStates _activeMoveState = ENpcMovementStates.Running;

    [Header("Agent Settings")] [SerializeField]
    private bool _enableAgent = true;

    [Header("Patrol Settings")]
    // General coroutine used for patrolling
    private Coroutine _patrolCoroutine;

    [Header("Waiting Settings")] [SerializeField]
    private bool _enableWaiting;

    [SerializeField] private float _waitTime;

    [Space] [Header("Patrol points")] [SerializeField]
    private PatrolPoint[] _patrolPoints;

    [SerializeField, ReadOnly] private int _currentPatrolIndex = 0;
    [SerializeField, ReadOnly] private PatrolPointActionValues[] _activePatrolPointActionValues;
    [SerializeField, ReadOnly] private PatrolPoint _activePatrolPoint;

    [Space] [Header("States")] 
    [SerializeField] private ENpcAiState _npcAiState = ENpcAiState.Idle;
    [SerializeField, ReadOnly] private ENpcAiState _lastActiveNpcAiState = ENpcAiState.Idle;

    [Space] [Header("Random Patrol")] 
    [SerializeField] private float _randomPatrolDistance = 10f;
    [SerializeField] private bool _patrolAllLayers; // If true, will use all NavMesh areas for random patrol
    // The name of the NavMesh area to use for random patrol
    [SerializeField] private string _navmeshLayerName = "Walkable";

    [SerializeField, ReadOnly] private Vector3 _randomPatrolDestination;
    [SerializeField] private GameObject _randomPatrolPointPrefab;

    [SerializeField, ReadOnly] private GameObject _activeRandomPatrolPoint;


    [Space] [Header("Follow Target")] 
    [SerializeField] private float _followTargetStopDistance = 3f;

    [SerializeField] private Transform _followTargetTransform;

    [Space] [Header("--  Debugging  --")] 
    [SerializeField] private Transform _agentDestinationTransform;

    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _velocityNormalizedMagnitude;
    [SerializeField] private TextMeshPro _stateText;
    [SerializeField, ReadOnly] private bool _hasRotationTarget;
    [SerializeField, ReadOnly] private Quaternion _rotationTarget;
    [SerializeField, ReadOnly] private float _rotationSpeed = 5f;
    
    // todo: interaction component stuff
    [SerializeField] private TalkUIManager _talkUIManager;
    [SerializeField, ReadOnly] private GameObject _interactor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _animationController = GetComponent<NpcAnimationController>();
    }

    private void Start()
    {
        if (_enableAgent)
        {
            _activeMoveState = _startMoveState;
            TriggerCurrentAiState();
            ChangeMovementSpeed(_startMoveState);
            _navMeshAgent.avoidancePriority = Random.Range(30, 70);
        }
        else
        {
            // Disable the NavMeshAgent if not enabled
            _navMeshAgent.enabled = false;
            _activeMoveState = ENpcMovementStates.Stationary; // Set to stationary if agent is not enabled
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Setting debug values
        _velocity = _navMeshAgent.velocity;
        _velocityNormalizedMagnitude = KMath.Normalize(
            _velocity.magnitude,
            0f,
            _npcMovementValues.runSpeed);

        // Update the movement state based on the normalized velocity magnitude
        _animationController.UpdateAnimator(_velocityNormalizedMagnitude);

        // Update rotation if active
        if (_hasRotationTarget)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _rotationTarget, Time.deltaTime * _rotationSpeed);
        }
    }

    private void TriggerCurrentAiState()
    {
        // Stop any existing patrol coroutine
        if (_patrolCoroutine != null)
        {
            StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }

        // To be safe since it's changes in specific states.
        _navMeshAgent.stoppingDistance = 0f;
        _navMeshAgent.enabled = true; // Ensure the agent is enabled

        switch (_npcAiState)
        {
            case ENpcAiState.Idle:
                _navMeshAgent.enabled = false;
                break; // Currently does nothing
            case ENpcAiState.FollowPatrolPoints:
                _patrolCoroutine = StartCoroutine(ContinuePointPatrol());
                break;
            case ENpcAiState.FollowRandomPatrol:
                _patrolCoroutine = StartCoroutine(ContinueRandomPatrol());
                break;
            case ENpcAiState.FollowTarget:
                _patrolCoroutine = StartCoroutine(ContinueFollowTarget());
                break;

            default: throw new ArgumentOutOfRangeException();
        }

        // Update the state text for debugging
        if (_stateText)
        {
            _stateText.text = _npcAiState.ToString();
        }
    }

    private IEnumerator ContinueRandomPatrol()
    {
        if (_npcAiState != ENpcAiState.FollowRandomPatrol) yield break;

        // Move to the next patrol point
        if (_enableWaiting)
        {
            yield return new WaitForSeconds(_waitTime);
        }

        // If we don't have a random patrol point, instantiate one
        if (!_activeRandomPatrolPoint)
        {
            int areaIndex = NavMesh.GetAreaFromName(_navmeshLayerName);
            int areaMask = 1 << areaIndex;

            if (_patrolAllLayers)
            {
                areaMask = NavMesh.AllAreas;
            }

            _randomPatrolDestination = RandomNavSphere(transform.position, _randomPatrolDistance, areaMask);
            _randomPatrolDestination.y += 1f;
            _activeRandomPatrolPoint =
                Instantiate(_randomPatrolPointPrefab, _randomPatrolDestination, Quaternion.identity);
        }

        MoveTo(_randomPatrolDestination);
    }

    private IEnumerator ContinueFollowTarget()
    {
        if (_npcAiState != ENpcAiState.FollowTarget) yield break;
        if (!_followTargetTransform) yield break;

        _navMeshAgent.stoppingDistance = _followTargetStopDistance;

        MoveTo(_followTargetTransform.position);
        // Wait for a short duration before checking again for the updated target location
        yield return new WaitForSeconds(0.5f);

        // Update the speed based on the distance to the target
        UpdateSpeedBasedOnDistance();

        // Basically a loop to keep following the target
        TriggerCurrentAiState();
    }

    private void UpdateSpeedBasedOnDistance()
    {
        // Base the movement speed on the distance to the target
        float distanceToTarget = Vector3.Distance(transform.position, _followTargetTransform.position);

        // Debug.Log(distanceToTarget);

        if (distanceToTarget < 4f)
        {
            ChangeMovementSpeed(ENpcMovementStates.Walking); // Walk towards the target
        }
        else if (distanceToTarget < 7f)
        {
            ChangeMovementSpeed(ENpcMovementStates.Running); // Sprint towards the target
        }
        else
        {
            ChangeMovementSpeed(ENpcMovementStates.Sprinting); // Sprint towards the target
        }
    }

    private void ChangeMovementSpeed(ENpcMovementStates newState)
    {
        if (!_npcMovementValues) return;
        if (!_navMeshAgent) return;

        _navMeshAgent.speed = newState switch
        {
            ENpcMovementStates.Stationary => 0f,
            ENpcMovementStates.Walking => _npcMovementValues.walkSpeed,
            ENpcMovementStates.Running => _npcMovementValues.runSpeed,
            ENpcMovementStates.Sprinting => _npcMovementValues.sprintSpeed,
            _ => throw new ArgumentOutOfRangeException()
        };

        _activeMoveState = newState; // Update the active movement state
    }

    private IEnumerator ContinuePointPatrol()
    {
        if (_npcAiState != ENpcAiState.FollowPatrolPoints) yield break;
        if (_patrolPoints.Length == 0) yield break;

        if (_activePatrolPoint && _activePatrolPoint.hasRotationAction)
        {
            // Rotate towards the specified direction
            _rotationTarget = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                _activePatrolPoint.yRotationDirection,
                transform.rotation.eulerAngles.z);

            _rotationSpeed = _activePatrolPoint.rotationSpeed;
            _hasRotationTarget = true; // Set the rotation target flag
            yield return new WaitForSeconds(_activePatrolPoint.rotationActionDuration);
            _hasRotationTarget = false; // Reset the rotation target flag
            _rotationSpeed = 5f; // Reset to default rotation speed, not really needed but for safety
        }

        if (_activePatrolPointActionValues != null)
        {
            foreach (var patrolPointValue in _activePatrolPointActionValues)
            {
                // Check if the patrol point value is null or not set
                if (!patrolPointValue) continue;

                // If the patrol point has an action, perform it
                _animationController.UpdateAnimationAction(patrolPointValue.actionClip);
                yield return new WaitForSeconds(patrolPointValue.actionDuration);
                _animationController.UpdateAnimationAction(null);

                // Wait for the action completion delay before proceeding if specified
                if (patrolPointValue.actionCompletionDelay.Equals(0f)) continue;

                yield return new WaitForSeconds(patrolPointValue.actionCompletionDelay);
            }
        }

        _activePatrolPointActionValues = null; // Reset the action values after performing the actions

        // Move to the next patrol point
        if (_enableWaiting)
        {
            yield return new WaitForSeconds(_waitTime);
        }

        // Stop patrolling if only one point is available but move to that location
        if (_patrolPoints.Length == 1)
        {
            _currentPatrolIndex = 0; // Reset to the first patrol point
            _npcAiState = ENpcAiState.Idle; // Stop patrolling
        }

        // Clear the current active patrol point till we reach the next one
        _activePatrolPoint = null;
        MoveTo(_patrolPoints[_currentPatrolIndex].transform.position);
    }

    // This method should be called when the agent reaches a patrol point
    public void OnPatrolPointReached(PatrolPoint patrolPoint)
    {
        // Check if the current index is out of bounds
        if (_currentPatrolIndex < 0 || _currentPatrolIndex >= _patrolPoints.Length) return;
        // Simple null check to ensure the patrol point exists
        if (_patrolPoints[_currentPatrolIndex] == null) return;
        // Check if the patrol point is the one we are currently targeting
        if (_patrolPoints[_currentPatrolIndex] != patrolPoint) return;

        // Store the current active patrol point
        _activePatrolPoint = patrolPoint;

        // Check and perform patrol point action
        if (patrolPoint.patrolPointActions is { Length: > 0 })
        {
            _activePatrolPointActionValues = patrolPoint.patrolPointActions;
        }
        else
        {
            _activePatrolPointActionValues = null; // Reset if no actions are available
        }

        if (_currentPatrolIndex + 1 >= _patrolPoints.Length)
        {
            _currentPatrolIndex = 0; // Reset to the first patrol point
        }
        else
        {
            _currentPatrolIndex++; // Move to the next patrol point
        }

        TriggerCurrentAiState();
    }

    // This method should be called when the agent reaches the randomized point
    public void OnRandomPatrolPointReached(GameObject randomPatrolPoint)
    {
        // Check if the patrol point is the one we are currently targeting
        if (randomPatrolPoint != _activeRandomPatrolPoint) return;

        // Destroy the random patrol point object
        Destroy(randomPatrolPoint.gameObject);

        // Reset the random patrol destination
        _randomPatrolDestination = Vector3.zero;
        _activeRandomPatrolPoint = null;

        // Retrigger the current AI state to continue
        TriggerCurrentAiState();
    }

    private void MoveTo(Vector3 destination)
    {
        if (!_navMeshAgent) return;

        _navMeshAgent.SetDestination(destination);
    }


    [Button]
    private void DebugMoveToTarget()
    {
        if (!_agentDestinationTransform) return;

        MoveTo(_agentDestinationTransform.position);
    }

    [Button]
    private void DebugUpdateAiState()
    {
        TriggerCurrentAiState();
    }

    [Button]
    private void DebugStopAiMovement()
    {
        if (_navMeshAgent == null) return;
        if (_npcAiState == ENpcAiState.Idle) return; // No need to stop if already idle
        _navMeshAgent.ResetPath();
        _lastActiveNpcAiState = _npcAiState; // Store the current AI state before stopping
        _npcAiState = ENpcAiState.Idle; // Set to idle state
        TriggerCurrentAiState();
    }

    [Button]
    private void DebugResumeAiMovement()
    {
        if (_navMeshAgent == null) return;
        if (_npcAiState == _lastActiveNpcAiState) return; // No need to resume if already in the last state
        _npcAiState = _lastActiveNpcAiState; // Restore the last AI state
        TriggerCurrentAiState();
    }

    [Button]
    private void DebugEnableRunning()
    {
        // Update state and agent speed
        if (!_npcMovementValues) return;
        if (!_navMeshAgent) return;
        ChangeMovementSpeed(ENpcMovementStates.Running);
    }

    [Button]
    private void DebugEnableWalking()
    {
        // Update state and agent speed
        if (!_npcMovementValues) return;
        if (!_navMeshAgent) return;
        ChangeMovementSpeed(ENpcMovementStates.Walking);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layermask);

        return navHit.position;
    }

    public void Interact(GameObject interactor)
    { 
        Debug.Log("Interact");
        // Check if the NPC is in a state that allows interaction but we assume it is always allowed for now
        DebugStopAiMovement();
        
        // Store the interactor so we know who is interacting with us
        _interactor = interactor;
        
        // Face the interactor
        float targetYRotation = Quaternion.LookRotation(interactor.transform.position - transform.position).eulerAngles.y;
        
        // Rotate towards the specified direction
        _rotationTarget = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            targetYRotation,
            transform.rotation.eulerAngles.z);

        _rotationSpeed = 2f;
        _hasRotationTarget = true; // Set the rotation target flag
        
        // Tell the player to use the talk angle camera
        if (interactor.TryGetComponent(out PlayerController playerController))
        {
            playerController.ToggleTalkInteraction(true);
        }
        
        // Set the talk UI interactor
        if (_talkUIManager)
        {
            _talkUIManager.SetInteractor(StopInteraction);
            _talkUIManager.SetTalkUIVisibility(true);
        }
    }
    
    private void StopInteraction()
    { 
        if (_interactor == null) // this should never happen but just in case
        {
            Debug.LogWarning("No interactor found to stop interaction.");
            return;
        }
        
        Debug.Log("Stopping Interact");
        // Check if the NPC is in a state that allows interaction but we assume it is always allowed for now
        DebugResumeAiMovement();
        
        _hasRotationTarget = false; // Set the rotation target flag
        
        // Tell the player to use the talk angle camera
        if (_interactor.TryGetComponent(out PlayerController playerController))
        {
            playerController.ToggleTalkInteraction(false);
        }
        
        // Update the ui visibility
        if (_talkUIManager)
        {
            _talkUIManager.SetTalkUIVisibility(false);
        }
    }
}
using System;
using System.Collections;
using Khaled.MathLib;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NpcAnimationController))]
public class NpcController : MonoBehaviour
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
    }
    
    [Header("Movement Values")]
    [SerializeField]
    private NpcMovementValues _npcMovementValues;

    [SerializeField] private ENpcMovementStates _startMoveState = ENpcMovementStates.Running;
    [SerializeField, ReadOnly] private ENpcMovementStates _activeMoveState = ENpcMovementStates.Running;


    [Header("Patrol Settings")]
    // General coroutine used for patrolling
    private Coroutine _patrolCoroutine;
    
    [Header("Waiting Settings")]
    [SerializeField] private bool _enableWaiting;
    [SerializeField] private float _waitTime;
    
    [Space]
    [Header("Patrol points")]
    [SerializeField] private PatrolPoint[] _patrolPoints;
    [SerializeField, ReadOnly] private int _currentPatrolIndex = 0;
    
    [Space]
    [Header("States")]
    [SerializeField] private ENpcAiState _npcAiState = ENpcAiState.Idle;
    [SerializeField, ReadOnly] private ENpcAiState _lastActiveNpcAiState = ENpcAiState.Idle;
    
    [Space]
    [Header("Random Patrol")]
    [SerializeField] private float _randomPatrolDistance = 10f;
    [SerializeField, ReadOnly] private Vector3 _randomPatrolDestination;
    [SerializeField] private GameObject _randomPatrolPointPrefab;
    [SerializeField, ReadOnly] private GameObject _activeRandomPatrolPoint;
    
    [Space]
    [Header("Follow Target")]
    [SerializeField] private float _followTargetStopDistance = 3f;
    [SerializeField] private Transform _followTargetTransform;
    
    [Space]
    [Header("--  Debugging  --")] 
    [SerializeField] private Transform _agentDestinationTransform;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _velocityNormalizedMagnitude;
    [SerializeField] private TextMeshPro _stateText;


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
        _activeMoveState = _startMoveState;
        TriggerCurrentAiState();
        ChangeMovementSpeed(_startMoveState);
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
        
        switch (_npcAiState)
        {
            case ENpcAiState.Idle: break; // Currently does nothing
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
            _randomPatrolDestination = RandomNavSphere(transform.position, _randomPatrolDistance, NavMesh.AllAreas);
            _randomPatrolDestination.y += 1f;
            _activeRandomPatrolPoint = Instantiate(_randomPatrolPointPrefab, _randomPatrolDestination, Quaternion.identity);
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
        
        // Basically a loop to keep following the target
        TriggerCurrentAiState();
    }

    // Update is called once per frame
    void Update()
    {
        // Setting debug values
        _velocity = _navMeshAgent.velocity;
        _velocityNormalizedMagnitude = KMath.Normalize(
            KMath.Magnitude(_velocity),
            0f,
            _npcMovementValues.runSpeed);
        
        // Update the movement state based on the normalized velocity magnitude
        _animationController.UpdateAnimator(_velocityNormalizedMagnitude);
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
            _ => throw new ArgumentOutOfRangeException()
        };
        
        _activeMoveState = newState; // Update the active movement state
    }

    private IEnumerator ContinuePointPatrol()
    {
        if (_npcAiState != ENpcAiState.FollowPatrolPoints) yield break;
        if (_patrolPoints.Length == 0) yield break;

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
    
    private Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask) {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
           
        randomDirection += origin;
           
        NavMeshHit navHit;
           
        NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask);
           
        return navHit.position;
    }
}
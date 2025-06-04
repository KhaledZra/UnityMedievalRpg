using System;
using System.Collections;
using Khaled.MathLib;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class NpcController : MonoBehaviour
{
    private CapsuleCollider _capsuleCollider;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rigidbody;

    // todo: move to NpcAnimationHandler.cs
    // [Header("Movement Settings")]
    private bool _isWalking;

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
    [Header("--  Debugging  --")] [SerializeField] private Transform _agentDestinationTransform;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _velocityNormalizedMagnitude;
    
    private enum ENpcAiState
    {
        Idle = 0,
        FollowPatrolPoints = 1,
        FollowRandomPatrol = 2,
        FollowTarget = 3,
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        TriggerCurrentAiState();
    }

    private void TriggerCurrentAiState()
    {
        // Stop any existing patrol coroutine
        if (_patrolCoroutine != null)
        {
            StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }
        
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
                // todo: follow function call here!
                break;
            
            default: throw new ArgumentOutOfRangeException();
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

    // Update is called once per frame
    void Update()
    {
        // Setting debug values
        _velocity = _navMeshAgent.velocity;
        _velocityNormalizedMagnitude = KMath.Normalize(
            KMath.Magnitude(_velocity),
            0f,
            _navMeshAgent.speed);

        UpdateAnimator();
    }

    
    // Refactor to NpcAnimationHandler.cs
    private void UpdateAnimator()
    {
        if (!_velocityNormalizedMagnitude.Equals(0f))
        {
            _animator.SetBool("IsMoving", true);
            // this is mostly for testing. I need to find a walk animation to change to it instead and add states
            if (_isWalking)
            {
                _animator.SetFloat("AnimationSpeed", _velocityNormalizedMagnitude * 0.5f);
                _navMeshAgent.speed = 2.5f;
            }
            else
            {
                _animator.SetFloat("AnimationSpeed", _velocityNormalizedMagnitude);
                _navMeshAgent.speed = 5f;
            }
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }
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
        if (_agentDestinationTransform == null) return;

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
    
    private Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask) {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
           
        randomDirection += origin;
           
        NavMeshHit navHit;
           
        NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask);
           
        return navHit.position;
    }
}
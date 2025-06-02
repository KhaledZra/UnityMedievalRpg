using System;
using System.Collections;
using Khaled.MathLib;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Movement Settings")] [SerializeField]
    private bool _isWalking;

    [Header("Patrol Settings")] 
    [SerializeField] private bool _enablePatrolling;
    [SerializeField] private bool _enableWaiting;
    [SerializeField] private float _waitTime;
    [SerializeField] private PatrolPoint[] _patrolPoints;
    [SerializeField, ReadOnly] private int _currentPatrolIndex = 0;

    [Header("Debugging")] [SerializeField] private Transform _agentDestinationTransform;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _velocityNormalizedMagnitude;


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
        ContinuePatrol();
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

    private void FixedUpdate()
    {
        // Handle physics-related updates here
    }

    private void ContinuePatrol()
    {
        if (!_enablePatrolling) return;
        if (_patrolPoints.Length == 0) return;

        // Stop patrolling if only one point is available but move to that location
        if (_patrolPoints.Length == 1)
        {
            MoveTo(_patrolPoints[0].transform.position);
            _enablePatrolling = false;
            return;
        }

        // Move to the next patrol point
        if (_enableWaiting)
        {
            Debug.LogWarning(_waitTime);
            StartCoroutine(WaitOnMoveTo());
        }
        else
        {
            MoveTo(_patrolPoints[_currentPatrolIndex].transform.position);
        }
    }

    private IEnumerator WaitOnMoveTo()
    {
        yield return new WaitForSeconds(_waitTime);
        MoveTo(_patrolPoints[_currentPatrolIndex].transform.position);
    }

    // This method should be called when the agent reaches a patrol point
    public void OnPatrolPointReached(PatrolPoint patrolPoint)
    {
        if (!_enablePatrolling) return;
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

        // Continue patrolling
        ContinuePatrol();
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
}
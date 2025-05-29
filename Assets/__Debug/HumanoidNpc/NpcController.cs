using Khaled.MathLib;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NpcController : MonoBehaviour
{
    private CapsuleCollider _capsuleCollider;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    [Header("Debugging")] [SerializeField] private Transform _agentDestinationTransform;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _velocityNormalizedMagnitude;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
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
            _animator.SetFloat("AnimationSpeed", _velocityNormalizedMagnitude);
        }
        else
        {
            // If the agent is not moving, you can set the animator to idle or any other state
            _animator.SetBool("IsMoving", false);
        }
    }

    private void FixedUpdate()
    {
        // Handle physics-related updates here
    }

    private void MoveTo(Vector3 destination)
    {
        if (_navMeshAgent == null) return;

        _navMeshAgent.SetDestination(destination);
    }
    
    [Button]
    private void DebugMoveToTarget()
    {
        if (_agentDestinationTransform == null) return;

        MoveTo(_agentDestinationTransform.position);
    }
}
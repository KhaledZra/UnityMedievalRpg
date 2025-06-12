using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SphereCollider))]
public class PatrolPoint : MonoBehaviour
{
    private SphereCollider _sphereCollider;
    
    [Header("Patrol Point Actions")]
    [SerializeField] public PatrolPointActionValues[] patrolPointActions;
    [SerializeField] public bool hasRotationAction = false;
    [SerializeField, ReadOnly] public float yRotationDirection = 0f;
    [SerializeField] public float rotationActionDuration = 2f;
    [SerializeField] public float rotationSpeed = 5f;
    

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        yRotationDirection = transform.eulerAngles.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the 
        if (other.gameObject.TryGetComponent(out NpcController npcController))
        {
            // Notify the NPC controller that it has reached this patrol point
            npcController.OnPatrolPointReached(this);
        }
    }
}

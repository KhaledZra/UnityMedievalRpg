using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SphereCollider))]
public class PatrolPoint : MonoBehaviour
{
    private SphereCollider _sphereCollider;
    
    [Header("Patrol Point Actions")]
    [SerializeField] public PatrolPointActionValues patrolPointActions;
    [SerializeField] public bool hasActionActive;
    

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    
    private void Start()
    {
        hasActionActive = patrolPointActions;
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

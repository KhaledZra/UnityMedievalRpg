using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RandomPatrolPoint : MonoBehaviour
{
    private SphereCollider _sphereCollider;

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PatrolPoint: OnTriggerEnter called with " + other.gameObject.name);
        // Check if the collider belongs to the 
        if (other.gameObject.TryGetComponent(out NpcController npcController))
        {
            // Notify the NPC controller that it has reached this patrol point
            npcController.OnRandomPatrolPointReached(gameObject);
        }
    }
}

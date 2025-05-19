using _Game.Interfaces;
using _Game.Structs;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeEnemy : MonoBehaviour, IAttackable
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public SAttackResult OnAttack(SHitResult hitResult)
    {
        SAttackResult attackResult = new();

        // Check if the attacker is a player
        if (hitResult.Attacker.CompareTag("Player"))
        {
            // Apply a force to the cube enemy in the direction of the hit point
            Vector3 forceDirection = (hitResult.HitPoint - hitResult.Attacker.transform.position).normalized;
            
            _rigidbody.AddForce(forceDirection * 10f, ForceMode.Impulse);
            attackResult.TargetKilled = true;
        }
        else
        {
            // Handle other types of attackers if needed
            attackResult.TargetKilled = false;
        }

        return attackResult;
    }
}

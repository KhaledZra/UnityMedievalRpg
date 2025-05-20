using _Game.Interfaces;
using _Game.Structs;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthComponent))]
public class CubeEnemy : MonoBehaviour, IAttackable
{
    private Rigidbody _rigidbody;
    private HealthComponent _healthComponent;
    
    [SerializeField] private GameObject _baseGameObject;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _healthComponent = GetComponent<HealthComponent>();
    }

    public SAttackResult OnAttack(SHitResult hitResult)
    {
        SAttackResult attackResult = new() { TargetKilled = false };

        // Check if the attacker is a player
        if (hitResult.Attacker.CompareTag("Player"))
        {
            // Apply a force to the cube enemy in the direction of the hit point
            Vector3 forceDirection = (hitResult.HitPoint - hitResult.Attacker.transform.position).normalized;

            _rigidbody.AddForce(forceDirection * 10f, ForceMode.Impulse);
            
            // Apply damage to the cube enemy
            _healthComponent.ApplyDamage(hitResult.Damage);
            
            // Check if the cube enemy is dead
            if (_healthComponent.IsDead)
            {
                // Handle the death of the cube enemy
                attackResult.TargetKilled = true;
                
                Debug.Log("Cube enemy killed!");

                Destroy(_baseGameObject ? _baseGameObject : gameObject);
            }
        }

        return attackResult;
    }
}
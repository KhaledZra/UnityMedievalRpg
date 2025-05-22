using _Game.Interfaces;
using _Game.Structs;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FloatingTextHandler))]
public class CubeEnemy : MonoBehaviour, IAttackable
{
    private Rigidbody _rigidbody;
    private HealthComponent _healthComponent;
    private FloatingTextHandler _floatingTextHandler;

    [SerializeField] private GameObject _baseGameObject;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _healthComponent = GetComponent<HealthComponent>();
        _floatingTextHandler = GetComponent<FloatingTextHandler>();
    }

    public SAttackResult OnAttack(SHitResult hitResult)
    {
        SAttackResult attackResult = new() { TargetKilled = false };

        // Check if the attacker is a player
        if (!hitResult.Attacker.CompareTag("Player")) return attackResult;

        // Apply a force to the cube enemy in the direction of the hit point
        Vector3 forceDirection = (hitResult.HitPoint - hitResult.Attacker.transform.position).normalized;
        
        _rigidbody.AddForce(forceDirection * hitResult.HitForce, ForceMode.Impulse);

        // Apply damage to the cube enemy
        _healthComponent.ApplyDamage(hitResult.Damage);
        
        // Spawn floating text
        Quaternion rotation = Quaternion.LookRotation(hitResult.HitPoint - hitResult.Attacker.transform.position);
        rotation.x = 0f;
        rotation.z = 0f;
        // Play floating at location instead
        _floatingTextHandler.SpawnFloatingTextAtPosition(hitResult.Damage, Color.red, transform.position, rotation);

        // Check if the cube enemy is dead
        if (!_healthComponent.IsDead) return attackResult;
        
        // Handle the death of the cube enemy
        attackResult.TargetKilled = true;

        Debug.Log("Cube enemy killed!");

        Destroy(_baseGameObject ? _baseGameObject : gameObject);
        return attackResult;
    }
}
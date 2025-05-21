using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private HealthValues _healthValues;
    [SerializeField] private float _currentHealth;
    
    [SerializeField] private HealthbarUIComponent _healthBarUI;
    
    public bool IsDead => _currentHealth <= 0f;

    private void Start()
    {
        _currentHealth = _healthValues.maxHealth;
        
        // Initialize the healthbar UI
        _healthBarUI?.SetVisible(false);
        _healthBarUI?.UpdateHealthBar(_currentHealth, _healthValues.maxHealth);
        
    }

    public void ApplyDamage(float damage)
    {
        if (IsDead)
        {
            Debug.Log("Already dead");
            return;
        }
        
        _currentHealth -= damage;
        
        // Update the healthbar UI if it's attached
        _healthBarUI?.SetVisible(true); // Show the health bar when taking damage
        _healthBarUI?.UpdateHealthBar(_currentHealth, _healthValues.maxHealth);
    }
}

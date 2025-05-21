using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthValues", menuName = "Scriptable Objects/HealthValues")]
public class HealthValues : ScriptableObject
{
    public int maxHealth; // Max health
    
    // Saved values
    [ReadOnly, SerializeField] private int _defaultMaxHealth = 100;

    private void OnEnable()
    {
        Reset();
    }

    [Button]
    private void Reset()
    {
        // Set default values for the movement parameters
        maxHealth = _defaultMaxHealth;
    }
    
    [Button]
    private void Save()
    {
        // Save the current values to the scriptable object
        _defaultMaxHealth = maxHealth;
    }
}

using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcMovementValues", menuName = "Scriptable Objects/NpcMovementValues")]
public class NpcMovementValues : ScriptableObject
{
    public float walkSpeed; // Speed when walking
    public float runSpeed; // Speed when running
    public float sprintSpeed; // Speed when running
    
    // Saved values
    [ReadOnly, SerializeField] private float _defaultWalkSpeed = 2.5f;
    [ReadOnly, SerializeField] private float _defaultRunSpeed = 5f;
    [ReadOnly, SerializeField] private float _defaultSprintSpeed = 7.5f;
    
    private void OnEnable()
    {
        Reset();
    }

    [Button]
    private void Reset()
    {
        // Set default values for the movement parameters
        walkSpeed = _defaultWalkSpeed;
        runSpeed = _defaultRunSpeed;
        sprintSpeed = _defaultSprintSpeed;
    }
    
    [Button]
    private void Save()
    {
        // Save the current values to the scriptable object
        _defaultWalkSpeed = walkSpeed;
        _defaultRunSpeed = runSpeed;
        _defaultSprintSpeed = sprintSpeed;
    }
}

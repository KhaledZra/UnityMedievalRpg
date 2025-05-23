using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementValues", menuName = "Scriptable Objects/MovementValues")]
public class MovementValues : ScriptableObject
{
    public float walkSpeed; // Speed when walking
    public float sprintSpeed; // Speed when sprinting
    public float crouchSpeed; // Speed when crouching
    public float jumpForce; // Force applied when jumping
    public float gravityValue; // Gravity value applied to the player
    public float accelerationSpeed; // Speed at which the player accelerates
    public float dragSpeed = 5f; // Speed at which the player decelerates
    
    // Saved values
    [ReadOnly, SerializeField] private float _defaultWalkSpeed = 5f;
    [ReadOnly, SerializeField] private float _defaultSprintSpeed = 10f;
    [ReadOnly, SerializeField] private float _defaultCrouchSpeed = 2f;
    [ReadOnly, SerializeField] private float _defaultJumpForce = 1f;
    [ReadOnly, SerializeField] private float _defaultGravityValue = -9.81f;
    [ReadOnly, SerializeField] private float _defaultAccelerationSpeed = 10f;
    [ReadOnly, SerializeField] private float _defaultDragSpeed = 5f;

    private void OnEnable()
    {
        Reset();
    }

    [Button]
    private void Reset()
    {
        // Set default values for the movement parameters
        walkSpeed = _defaultWalkSpeed;
        sprintSpeed = _defaultSprintSpeed;
        crouchSpeed = _defaultCrouchSpeed;
        jumpForce = _defaultJumpForce;
        gravityValue = _defaultGravityValue;
        accelerationSpeed = _defaultAccelerationSpeed;
        dragSpeed = _defaultDragSpeed;
    }
    
    [Button]
    private void Save()
    {
        // Save the current values to the scriptable object
        _defaultWalkSpeed = walkSpeed;
        _defaultSprintSpeed = sprintSpeed;
        _defaultCrouchSpeed = crouchSpeed;
        _defaultJumpForce = jumpForce;
        _defaultGravityValue = gravityValue;
        _defaultAccelerationSpeed = accelerationSpeed;
        _defaultDragSpeed = dragSpeed;
    }
}

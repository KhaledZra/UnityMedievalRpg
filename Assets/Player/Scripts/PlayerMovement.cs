using System;
using UnityEngine;

// TODO: handle delayed input actions
// TODO: Refactor player state variables into a single state object "StateMachine"
// TODO: camera sway

// TODO: Jump and Gravity speed are a little broken RN. FIX THIS!!!!!!!!!!!!!!!!!!!

public class PlayerMovement : MonoBehaviour
{
    // Data Objects
    [Header("Data Objects")]
    [SerializeField]
    private MovementValues _movementValues;
    
    // Component references
    [Header("Components")]
    [SerializeField]
    private CharacterController _characterController;
    
    [SerializeField]
    private Transform _cameraTransform;
    
    // Player state variables
    private bool _isJumping;
    private bool _isSprinting;
    private bool _isCrouching;
    
    private float _currentMoveSpeed
    {
        get
        {
            // Determine the current move speed based on player state
            if (_isCrouching) return _movementValues.crouchSpeed; 
            if (_isSprinting) return _movementValues.sprintSpeed;
            
            return _movementValues.walkSpeed;
        }
    }
    
    // for debugging
    [Header("Debugging")] [field: SerializeField]
    public Vector3 Velocity;
    
    private void OnEnable()
    {
        // Bind the input actions
        PlayerInputHandler.Instance.JumpInputAction += OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction += OnSprintInput;
        PlayerInputHandler.Instance.CrouchInputAction += OnCrouchInput;
    }
    
    private void OnDisable()
    {
        // Unbind the input actions
        PlayerInputHandler.Instance.JumpInputAction -= OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction -= OnSprintInput;
        PlayerInputHandler.Instance.CrouchInputAction -= OnCrouchInput;
    }

    private void FixedUpdate()
    {
        MovementUpdate();
    }

    private void MovementUpdate()
    {
        Vector3 cameraForwardXZ = new Vector3(_cameraTransform.transform.forward.x, 0f, _cameraTransform.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_cameraTransform.transform.right.x, 0f, _cameraTransform.transform.right.z).normalized;
        Vector3 movementDirection = cameraRightXZ * 
            PlayerInputHandler.Instance.MovementInputValue.x + cameraForwardXZ * PlayerInputHandler.Instance.MovementInputValue.y;

        Vector3 movementDelta = movementDirection * _movementValues.accelerationSpeed * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player
        Vector3 currentDrag = newVelocity.normalized * _movementValues.dragSpeed * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > _movementValues.dragSpeed * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, _currentMoveSpeed);
        
        // Apply Jump if state is active
        newVelocity.y = ApplyJump();
        
        // Apply gravity
        newVelocity.y += _movementValues.gravityValue * Time.deltaTime;

        // Move character (Unity suggests only calling this once per tick)
        _characterController.Move(newVelocity * Time.deltaTime);
        
        Velocity = newVelocity;
    }
    
    private float ApplyJump()
    {
        if (_isJumping)
        {
            _isJumping = false; // Reset the jump flag
            return Mathf.Sqrt(_movementValues.jumpForce * -2.0f * _movementValues.gravityValue);
        }

        return 0f;
    }
    
    private void OnJumpInput()
    {
        // Handle jump input
        if (_characterController.isGrounded)
        {
            // Set the jump flag to true
            _isJumping = true;
        }
    }
    
    private void OnCrouchInput(bool wantsToCrouch)
    {
        // handle crouch action
        if (wantsToCrouch && !_isSprinting)
        {
            // Set the crouch flag to true and decrease the move speed
            _isCrouching = true;
            
            // shrink the player collider
            _characterController.height = 0.75f;
            transform.localScale = new Vector3(1f, 0.5f, 1f);
        }
        else if (!wantsToCrouch && _isCrouching)
        {
            // Reset the crouch flag to false and increase the move speed
            _isCrouching = false;
            
            // shrink the player collider
            _characterController.height = 2f; // Set the height of the character controller to crouch height
            transform.localScale = Vector3.one; // Set the scale of the player object to normal height
        }
    }

    private void OnSprintInput(bool wantsToSprint)
    {
        // handle sprint action
        if (wantsToSprint && _characterController.isGrounded && !_isCrouching)
        {
            // Set the sprint flag to true and increase the move speed
            _isSprinting = true;
        }
        else if (!wantsToSprint && _isSprinting)
        {
            // Reset the sprint flag to false and decrease the move speed
            _isSprinting = false;
        }
    }
}

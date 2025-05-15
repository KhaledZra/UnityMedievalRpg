using UnityEngine;

// TODO: handle delayed input actions
// TODO: Improve state machine to handle movement states better. keep sprinting and jumping separate from the movement state

public class PlayerMovement : MonoBehaviour
{
    // Data Objects
    [Header("Data Objects")] [SerializeField]
    private MovementValues _movementValues;

    // Component references
    [Header("Components")] [SerializeField]
    private CharacterController _characterController;

    [SerializeField] private Transform _cameraTransform;

    // Player state variables
    [Header("Player State")] public PlayerMovementState _playerMovementState;

    private float _verticalVelocity;
    private Vector3 _targetMoveDirection = Vector3.zero;

    private float CurrentMoveSpeed
    {
        get
        {
            // Determine the current move speed based on player state
            if (_playerMovementState.CurrentMovementState == PlayerMovementState.EPlayerMovementState.Sprinting)
                return _movementValues.sprintSpeed;

            return _movementValues.walkSpeed;
        }
    }

    // TODO: for debugging
    [Header("Debugging")] [field: SerializeField]
    public Vector3 Velocity;

    private void OnEnable()
    {
        // Bind the input actions
        PlayerInputHandler.Instance.JumpInputAction += OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction += OnSprintInput;
    }

    private void OnDisable()
    {
        // Unbind the input actions
        PlayerInputHandler.Instance.JumpInputAction -= OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction -= OnSprintInput;
    }

    private void FixedUpdate()
    {
        _playerMovementState.UpdateGroundState(_characterController, transform);
        _playerMovementState.UpdateMovementState(PlayerInputHandler.Instance.MovementInputValue);
        MovementUpdate();
    }

    private void MovementUpdate()
    {
        // Store input values
        Vector3 targetPosition = new Vector3(
            PlayerInputHandler.Instance.MovementInputValue.x,
            0,
            PlayerInputHandler.Instance.MovementInputValue.y);
        
        // Apply speed
        targetPosition *= CurrentMoveSpeed * Time.deltaTime;

        // Apply camera direction
        targetPosition = transform.TransformDirection(targetPosition);

        _targetMoveDirection = targetPosition;

        ApplyGravity();
        
        // Jump if state is active
        ApplyJump();
        
        // Apply vertical velocity
        _targetMoveDirection.y = _verticalVelocity * Time.deltaTime;
        
        // Move the character controller
        _characterController.Move(_targetMoveDirection);
        
        // Todo: debugging
        Velocity = PlayerInputHandler.Instance.MovementInputValue;
    }

    private void ApplyGravity()
    {
        // TODO: maybe clamp gravity so we don't fall too fast?
        // Reset vertical velocity
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            // Reset vertical velocity if grounded
            _verticalVelocity = -2f;
        }
        
        // Apply gravity
        _verticalVelocity += _movementValues.gravityValue * Time.fixedDeltaTime;
    }
    
    private void ApplyJump()
    {
        if (!_playerMovementState.WantsToJump) return;
        
        _verticalVelocity = Mathf.Sqrt(_movementValues.jumpForce * -2.0f * _movementValues.gravityValue);
        // TODO: Testing to allow holding jump button
        _playerMovementState.WantsToJump = false;
    }

    private void OnJumpInput()
    {
        // Handle jump input
        if (_playerMovementState.InGroundState() && _characterController.isGrounded)
        {
            // Set the jump flag to true
            _playerMovementState.WantsToJump = true;
        }
    }

    private void OnSprintInput(bool wantsToSprint)
    {
        // handle sprint action
        if (wantsToSprint && _playerMovementState.InGroundState())
        {
            _playerMovementState.WantsToSprint = true;
        }
        else
        {
            _playerMovementState.WantsToSprint = false;
        }
    }
}
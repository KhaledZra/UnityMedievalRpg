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

    [SerializeField] private bool _isGrounded;
    [SerializeField] private LayerMask _groundLayerMask;

    // todo: move this to the PlayerMovementState class
    private bool _wantsToSprint;
    private bool _wantsToJump;

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

    // for debugging
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
        UpdateIsGrounded();
        UpdateMovementState();
        RBMovementUpdate();
    }

    private void UpdateIsGrounded()
    {
        // check and update the isGrounded state
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius,
            transform.position.z);
        
        _isGrounded = Physics.CheckSphere(spherePosition, _characterController.radius,
            _groundLayerMask, QueryTriggerInteraction.Ignore);
        
        // debug sphere
        DebugExtension.DebugWireSphere(spherePosition, Color.red, _characterController.radius);
    }

    private void RBMovementUpdate()
    {
        // Reset vertical velocity
        if (_isGrounded && _verticalVelocity < 0f)
        {
            // Reset vertical velocity if grounded
            _verticalVelocity = -2f;
        }
        
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
        
        // Apply gravity
        _verticalVelocity += _movementValues.gravityValue * Time.deltaTime;
        
        // Jump if state is active
        ApplyJump();
        
        _targetMoveDirection.y = _verticalVelocity * Time.deltaTime;
        
        // Move the character controller
        _characterController.Move(_targetMoveDirection);
        
        // Todo: debugging
        Velocity = PlayerInputHandler.Instance.MovementInputValue;
    }

    private void ApplyGravity()
    {
        if (_isGrounded)
        {
            
        }
    }


    // todo: move this to the PlayerMovementState class
    // todo: might switch to velocity based system in the future instead of input based
    private void UpdateMovementState()
    {
        // Jump/Falling state check is based on the character controller velocity
        // todo: needs refactoring, it's pretty sensitive to the velocity so if we are moving quickly downhill it
        // todo: could be seen as falling or running up a hill quickly could be jumping!
        // todo: i think the fix is a better isGrounded check since current seems flaky
        if (!_characterController.isGrounded) // Not grounded
        {
            if (_characterController.velocity.y < 0f) // Falling
            {
                _playerMovementState.SetMovementState(PlayerMovementState.EPlayerMovementState.Falling);
            }
            else if (_characterController.velocity.y > 0f) // Jumping
            {
                _playerMovementState.SetMovementState(PlayerMovementState.EPlayerMovementState.Jumping);
            }
        }
        // Player is not moving
        else if (Vector2.Equals(PlayerInputHandler.Instance.MovementInputValue, Vector2.zero))
        {
            // Idle
            _playerMovementState.SetMovementState(PlayerMovementState.EPlayerMovementState.Idle);
        }
        // Player is moving
        else
        {
            // Check if the player is sprinting
            if (_wantsToSprint)
            {
                // Sprinting
                _playerMovementState.SetMovementState(PlayerMovementState.EPlayerMovementState.Sprinting);
            }
            else
            {
                // Walking
                _playerMovementState.SetMovementState(PlayerMovementState.EPlayerMovementState.Walking);
            }
        }
    }

    private void MovementUpdate()
    {
        Vector3 cameraForwardXZ =
            new Vector3(_cameraTransform.transform.forward.x, 0f, _cameraTransform.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_cameraTransform.transform.right.x, 0f, _cameraTransform.transform.right.z)
            .normalized;
        Vector3 movementDirection = cameraRightXZ *
                                    PlayerInputHandler.Instance.MovementInputValue.x +
                                    cameraForwardXZ * PlayerInputHandler.Instance.MovementInputValue.y;

        Vector3 movementDelta = movementDirection * _movementValues.accelerationSpeed * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player
        Vector3 currentDrag = newVelocity.normalized * _movementValues.dragSpeed * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > _movementValues.dragSpeed * Time.deltaTime)
            ? newVelocity - currentDrag
            : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, CurrentMoveSpeed);

        // Apply gravity
        newVelocity.y = _characterController.velocity.y + _movementValues.gravityValue * Time.deltaTime;

        // Apply Jump if state is active
        newVelocity.y += ApplyJump();

        // Move character (Unity suggests only calling this once per tick)
        _characterController.Move(newVelocity * Time.deltaTime);

        Velocity = newVelocity;
    }

    // todo: refactor
    private float ApplyJump()
    {
        if (_wantsToJump)
        {
            _wantsToJump = false;
            _verticalVelocity = Mathf.Sqrt(_movementValues.jumpForce * -2.0f * _movementValues.gravityValue);
        }

        return 0f;
    }

    private void OnJumpInput()
    {
        // Handle jump input
        if (_characterController.isGrounded)
        {
            // Set the jump flag to true
            _wantsToJump = true;
        }
    }

    private void OnSprintInput(bool wantsToSprint)
    {
        // handle sprint action
        if (wantsToSprint && _characterController.isGrounded)
        {
            // Set the sprint flag to true and increase the move speed
            _wantsToSprint = true;
        }
        else if (!wantsToSprint && _playerMovementState.CurrentMovementState ==
                 PlayerMovementState.EPlayerMovementState.Sprinting)
        {
            // Reset the sprint flag to false and decrease the move speed
            _wantsToSprint = false;
        }
    }
}
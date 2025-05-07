using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // This script is attached to the player object and handles player movement and actions
    public InputActionAsset inputActions; // Reference to the InputActionAsset for handling input

    // Input actions
    private InputAction _moveAction; // Action for player movement
    private InputAction _jumpAction; // Action for player jumping
    private InputAction _sprintAction; // Action for player sprinting
    private InputAction _crouchAction; // Action for player crouching

    // Input action values
    private Vector2 _moveInputValue; // Store the movement input from the player
    
    // Data Objects
    public MovementValues movementValues; // Reference to the MovementValues scriptable object
    
    // // Player state variables
    private bool _wantsToJump; // Flag to check if the player wants to jump
    // private bool _wantsToSprint = false; // Flag to check if the player is sprinting
    // private bool _wantsToCrouch = false; // Flag to check if the player is crouching
    private bool _isSprinting;
    private bool _isCrouching;
    
    private float _currentMoveSpeed
    {
        get
        {
            // Determine the current move speed based on player state
            if (_isCrouching) return movementValues.crouchSpeed; 
            if (_isSprinting) return movementValues.sprintSpeed;
            
            return movementValues.walkSpeed;
        }
    }

    // Component references
    private CharacterController _characterController;

    // Current Velocity
    private Vector3 _velocity = Vector3.zero; // Store the current velocity of the player

    private void Awake()
    {
        // map the input actions to the player
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
        _crouchAction = InputSystem.actions.FindAction("Crouch");

        // Get the Rigidbody component attached to the player object
        _characterController = GetComponentInParent<CharacterController>();
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _moveInputValue = _moveAction.ReadValue<Vector2>(); // Read the movement input value from the action

        // handle jump action
        if (_jumpAction.WasPressedThisFrame())
        {
            // Handle jump action
            if (_characterController.isGrounded)
            {
                // Set the jump flag to true
                _wantsToJump = true;
            }
        }
        
        // handle sprint action
        if (_sprintAction.WasPressedThisFrame() && _characterController.isGrounded && !_isCrouching)
        {
            // Set the sprint flag to true and increase the move speed
            _isSprinting = true;
        }
        else if (_sprintAction.WasReleasedThisFrame() && _isSprinting)
        {
            // Reset the sprint flag to false and decrease the move speed
            _isSprinting = false;
        }
        
        // handle crouch action
        if (_crouchAction.WasPressedThisFrame() && !_isSprinting)
        {
            // Set the crouch flag to true and decrease the move speed
            _isCrouching = true;
            
            // shrink the player collider
            _characterController.height = 0.5f; // Set the height of the character controller to crouch height
            transform.localScale = new Vector3(1f, 0.5f, 1f); // Set the scale of the player object to crouch height
        }
        else if (_crouchAction.WasReleasedThisFrame() && _isCrouching)
        {
            // Reset the crouch flag to false and increase the move speed
            _isCrouching = false;
            
            // shrink the player collider
            _characterController.height = 2f; // Set the height of the character controller to crouch height
            transform.localScale = Vector3.one; // Set the scale of the player object to normal height
        }
    }

    private void FixedUpdate()
    {
        MovementUpdate();
    }

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void ApplyJump()
    {
        if (_wantsToJump)
        {
            _velocity.y = Mathf.Sqrt(movementValues.jumpForce * -2.0f * movementValues.gravityValue);
            _wantsToJump = false; // Reset the jump flag
        }
    }

    private void MovementUpdate()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0f;
        }
        
        // Calculate the movement direction based on input
        Vector3 moveDirection = new Vector3(_moveInputValue.x, 0, _moveInputValue.y);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        
        ApplyJump();
        
        // Apply gravity to the player
        _velocity.y += movementValues.gravityValue * Time.deltaTime;
        
        // Final movement vector
        Vector3 finalMove = (moveDirection * _currentMoveSpeed) + (_velocity.y * Vector3.up);
        
        // Figure out the direction the player is facing
        finalMove = transform.TransformDirection(finalMove);
        
        _characterController.Move(finalMove * Time.deltaTime);
    }
}

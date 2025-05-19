using _Game.Interfaces;
using _Game.Structs;
using UnityEngine;
using UnityEngine.Serialization;

// TODO: handle delayed input actions
// TODO: Improve state machine to handle movement states better. keep sprinting and jumping separate from the movement state

public class PlayerController : MonoBehaviour
{
    // Data Objects
    [Header("Data Objects")] [SerializeField]
    private MovementValues _movementValues;

    // Component references
    [Header("Components")] [SerializeField]
    private CharacterController _characterController;

    [SerializeField] private Transform _cameraTransform;

    // Player state variables
    [FormerlySerializedAs("_playerMovementState")] [Header("Player State")]
    public PlayerState PlayerState;

    private float _verticalVelocity;
    private Vector3 _targetMoveDirection = Vector3.zero;

    private float CurrentMoveSpeed
    {
        get
        {
            // Determine the current move speed based on player state
            if (PlayerState.CurrentMovementState == PlayerState.EPlayerMovementState.Sprinting)
                return _movementValues.sprintSpeed;

            return _movementValues.walkSpeed;
        }
    }

    // todo: attack values should be in a data object
    [Header("Attacking")]
    [SerializeField] private float _attackDistance = 10f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private LayerMask _attackableLayerMask;
    [SerializeField] private Transform _mainRaycastCamera;

    // TODO: for debugging
    [Header("Debugging")] [field: SerializeField]
    public Vector3 Velocity;

    private void OnEnable()
    {
        // Bind the input actions
        PlayerInputHandler.Instance.JumpInputAction += OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction += OnSprintInput;
        PlayerInputHandler.Instance.LeftAttackInputAction += OnLeftAttackInput;
        PlayerInputHandler.Instance.RightAttackInputAction += OnRightAttackInput;
    }

    private void OnDisable()
    {
        // Unbind the input actions
        PlayerInputHandler.Instance.JumpInputAction -= OnJumpInput;
        PlayerInputHandler.Instance.SprintInputAction -= OnSprintInput;
        PlayerInputHandler.Instance.LeftAttackInputAction -= OnLeftAttackInput;
        PlayerInputHandler.Instance.RightAttackInputAction -= OnRightAttackInput;
    }

    private void FixedUpdate()
    {
        PlayerState.UpdateGroundState(_characterController, transform);
        PlayerState.UpdateMovementState(PlayerInputHandler.Instance.MovementInputValue);
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
        if (!PlayerState.WantsToJump) return;

        _verticalVelocity = Mathf.Sqrt(_movementValues.jumpForce * -2.0f * _movementValues.gravityValue);
        // TODO: Testing to allow holding jump button
        PlayerState.WantsToJump = false;
    }

    private void OnJumpInput()
    {
        if (!PlayerState.InGroundState() && !_characterController.isGrounded)
            return;
        if (PlayerState.CurrentAttackState != PlayerState.EPlayerAttackState.Idle)
            return;

        // Set the jump flag to true
        PlayerState.WantsToJump = true;
    }

    private void OnSprintInput(bool wantsToSprint)
    {
        // handle sprint action
        if (wantsToSprint && PlayerState.InGroundState())
        {
            PlayerState.WantsToSprint = true;
        }
        else
        {
            PlayerState.WantsToSprint = false;
        }
    }

    // todo: left/right attack functions can be combined into one
    // todo: also the cast attack needs to be used called on animation events so it feels more responsive.
    private void OnLeftAttackInput()
    {
        // Already attacking
        if (PlayerState.CurrentAttackState != PlayerState.EPlayerAttackState.Idle ||
            !PlayerState.InGroundState())
            return;

        // Handle left attack input
        PlayerState.SetAttackState(PlayerState.EPlayerAttackState.LeftPunching);
    }

    private void OnRightAttackInput()
    {
        // Already attacking
        if (PlayerState.CurrentAttackState != PlayerState.EPlayerAttackState.Idle ||
            !PlayerState.InGroundState())
            return;

        // Handle left attack input
        PlayerState.SetAttackState(PlayerState.EPlayerAttackState.RightPunching);
    }

    private bool RaycastAttack(out RaycastHit raycastHit)
    {
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(_mainRaycastCamera.position, _mainRaycastCamera.forward, out raycastHit, _attackDistance,
                _attackableLayerMask))
        {
            Debug.DrawRay(_mainRaycastCamera.position, _mainRaycastCamera.forward * raycastHit.distance, Color.yellow, 10f);

            // Checks if the hit point is behind the player
            if (transform.InverseTransformPoint(raycastHit.point).z < 0)
            {
                Debug.Log("Hit behind the player");
                return false;
            }
            
            Debug.Log("Did Hit");
            return true;

            // todo: notify hit target to act on the hit
        }
        
        Debug.DrawRay(_mainRaycastCamera.position, _mainRaycastCamera.forward * _attackDistance, Color.white, 10f);
        Debug.Log("Did not Hit");
        return false;
    }

    // This is triggered when we want to start the attack raycast
    // Called from animation events:
    // "PunchLeft" animation clip
    // "PunchRight" animation clip
    public void OnAttackStart()
    {
        // If we have not hit result we simply return here
        if (!RaycastAttack(out RaycastHit raycastHit)) return;
        
        // Check if the hit object has an IAttackable component
        if (raycastHit.collider.gameObject.TryGetComponent(out IAttackable attackable))
        {
            // Create a hit result object
            SHitResult hitResult = new()
            {
                Attacker = gameObject,
                HitPoint = raycastHit.point,
                Damage = _attackDamage,
            };
            attackable?.OnAttack(hitResult);
        }
        else
        {
            Debug.Log("No IAttackable component found on the hit object.");
        }
    }
}
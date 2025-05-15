using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    [Header("Player Movement State")]
    [field: SerializeField]
    public EPlayerMovementState CurrentMovementState { get; private set; } = EPlayerMovementState.Idle;
    [field: SerializeField]
    public EPlayerGroundState CurrentGroundState { get; private set; } = EPlayerGroundState.Grounded;
    
    [Header("Requested States")]
    // Requested State variables
    // todo: needs a new system where we can save the state and transition back to it
    // todo so if we stop sprinting to jump, we can go back to sprinting on landing
    [SerializeField] public bool WantsToSprint;
    [SerializeField] public bool WantsToJump;
    
    [Header("Ground Layer Mask")]
    [SerializeField] private LayerMask _groundLayerMask;
    
    // // Seems redundant unless we are adding some logic to the setter?
    // public void SetMovementState(EPlayerMovementState newState)
    // {
    //     CurrentMovementState = newState;
    // }

    public void UpdateMovementState(Vector3 movementInput)
    {
        // Check the movement input and update the movement state
        if (movementInput.magnitude > 0.1f)
        {
            CurrentMovementState = WantsToSprint ?
                EPlayerMovementState.Sprinting :
                EPlayerMovementState.Walking;
        }
        else
        {
            CurrentMovementState = EPlayerMovementState.Idle;
        }
    }

    public void UpdateGroundState(CharacterController characterController, Transform playerTransform)
    {
        // check and update the isGrounded state
        Vector3 spherePosition = new Vector3(playerTransform.position.x, playerTransform.position.y - characterController.radius,
            playerTransform.position.z);
        
        bool isGrounded = Physics.CheckSphere(spherePosition, characterController.radius,
            _groundLayerMask, QueryTriggerInteraction.Ignore);
        
        // debug sphere
        DebugExtension.DebugWireSphere(spherePosition, Color.red, characterController.radius);
        
        if (isGrounded)
        {
            // Set the grounded state
            CurrentGroundState = EPlayerGroundState.Grounded;
        }
        else // Not grounded
        {
            if (characterController.velocity.y < 0f) // Falling
            {
                CurrentGroundState = EPlayerGroundState.Falling;
            }
            else if (characterController.velocity.y > 0f) // Jumping
            {
                CurrentGroundState = EPlayerGroundState.Jumping;
            }
            else // Safety case, shouldn't happen
            {
                CurrentGroundState = EPlayerGroundState.Grounded;
            }
        }
    }
    
    public bool InGroundState()
    {
        return CurrentGroundState == EPlayerGroundState.Grounded;
    }
    
    public enum EPlayerMovementState
    {
        Idle = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
        Strafing = 6,
    }
    
    public enum EPlayerGroundState
    {
        Grounded = 0,
        Jumping = 4,
        Falling = 5,
    }
}

using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    [field: SerializeField]
    public EPlayerMovementState CurrentMovementState { get; set; } = EPlayerMovementState.Idle;
    
    // Seems redundant unless we are adding some logic to the setter?
    public void SetMovementState(EPlayerMovementState newState)
    {
        CurrentMovementState = newState;
    }
    
    public bool InGroundState()
    {
        // todo: refactor this when a new ground state check is added
        return CurrentMovementState == EPlayerMovementState.Idle ||
               CurrentMovementState == EPlayerMovementState.Walking ||
               CurrentMovementState == EPlayerMovementState.Running ||
               CurrentMovementState == EPlayerMovementState.Strafing;
    }
    
    public enum EPlayerMovementState
    {
        Idle = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
        Jumping = 4,
        Falling = 5,
        Strafing = 6,
    }
}

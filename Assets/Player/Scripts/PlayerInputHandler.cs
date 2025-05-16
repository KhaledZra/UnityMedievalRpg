using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public static PlayerInputHandler Instance { get; private set; }
    
    private InputSystem_Actions _actions; // Reference to the InputSystem_Actions for handling input actions
    private InputSystem_Actions.PlayerActions _playerActions; // Reference to the player actions
    
    // Input action values
    public Vector2 MovementInputValue { get; private set; }
    public Vector2 LookInputValue { get; private set; }
    
    // Input action events
    public event Action JumpInputAction;
    public event Action<bool> SprintInputAction;
    public event Action LeftAttackInputAction;
    public event Action RightAttackInputAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Initialize the input system
        _actions = new InputSystem_Actions();
        _playerActions = _actions.Player;
        _playerActions.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        // Destroy Asset Object
        _actions.Dispose();
    }
    
    private void OnEnable()
    {
        _playerActions.Enable();
    }
    
    private void OnDisable()
    {
        _playerActions.Disable();
        
        _playerActions.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementInputValue = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInputValue = context.ReadValue<Vector2>();
    }

    public void OnLeftAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LeftAttackInputAction?.Invoke();
        }
    }
    
    public void OnRightAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RightAttackInputAction?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        // CrouchInputAction?.Invoke(context.ReadValueAsButton());
        
        // log crouch input
        Debug.Log($"Crouch Input: {context.ReadValueAsButton()}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpInputAction?.Invoke();
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintInputAction?.Invoke(context.ReadValueAsButton());
    }
}

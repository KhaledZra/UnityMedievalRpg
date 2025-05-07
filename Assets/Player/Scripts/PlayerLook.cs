using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions; // Reference to the InputActionAsset for handling input
    [SerializeField] public Transform playerBody; // Reference to the player body transform
    [SerializeField] public Transform playerEyes; // Reference to the player body transform

    // [field: SerializeField] public float MoveSpeed { get; set; } = 3f; // Speed of the player movement
    private InputAction _lookAction; // Action for player Look
    private Vector2 _lookInputValue; // Store the look input from the player
    public float lookSensitivity = 1f; // Sensitivity of the look input
    private Vector2 _lookDirection = Vector3.zero;
    private void Awake()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _lookInputValue = _lookAction.ReadValue<Vector2>(); // Read the look input value from the action
    }

    private void FixedUpdate()
    {
        LookUpdate();
    }
    
    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }
    
    private void LookUpdate()
    {
        // Calculate the look direction based on input
        // _lookInputValue = Vector2.ClampMagnitude(_lookInputValue, 1f);
        
        // Apply sensitivity to the look input
        _lookInputValue *= lookSensitivity;
        
        // Update the look direction based on input
        _lookDirection.x += _lookInputValue.x;
        _lookDirection.y += _lookInputValue.y;
        
        // Clamp the vertical look angle
        _lookDirection.y = Mathf.Clamp(_lookDirection.y, -80f, 80f);

        // Rotate Camera Y axis
        transform.rotation = Quaternion.Euler(-_lookDirection.y, _lookDirection.x, 0);
        // Rotate Player Body Y axis
        playerBody.rotation = Quaternion.Euler(0, _lookDirection.x, 0);
        // Rotate Player Eyes Y axis
        playerEyes.rotation = Quaternion.Euler(Mathf.Clamp(-_lookDirection.y, -40f, 40f), _lookDirection.x, 0);
    }
}

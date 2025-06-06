using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] public Transform playerBody; // Reference to the player body transform

    // Sensitivity of the look input
    public float lookSensitivity = 0.1f;

    // Helps to determine the look direction
    private Vector2 _lookDirection = Vector3.zero;
    private Vector2 _playerTargetRotation = Vector3.zero;

    private void FixedUpdate()
    {
        LookUpdate();
    }

    private void LookUpdate()
    {
        _lookDirection.x += PlayerInputHandler.Instance.LookInputValue.x * lookSensitivity;
        _lookDirection.y = Mathf.Clamp(
            _lookDirection.y - PlayerInputHandler.Instance.LookInputValue.y * lookSensitivity,
            -80f, 80f);

        // Player Body Rotation
        _playerTargetRotation.x = Mathf.LerpAngle(_playerTargetRotation.x, _lookDirection.x, Time.deltaTime * 10f);

        // Apply the rotation to the player body
        if (playerBody)
        {
            playerBody.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        }

        // Apply the rotation to the camera
        transform.rotation = Quaternion.Euler(_lookDirection.y, _lookDirection.x, 0f);
    }
}
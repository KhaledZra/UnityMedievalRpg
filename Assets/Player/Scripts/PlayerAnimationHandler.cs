using System;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    [Header("Animation")] [SerializeField] private Animator _animator;
    [SerializeField] private float _animationBlendSpeed = 0.02f;
    [SerializeField] private PlayerMovementState _playerMovementState;

    // So we don't have to use strings
    private static readonly int InputXHash = Animator.StringToHash("InputX");
    private static readonly int InputYHash = Animator.StringToHash("InputY");
    private static readonly int InputMagHash = Animator.StringToHash("InputMagnitude");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private Vector2 _currentBlendInput = Vector2.zero;

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isSprinting = _playerMovementState.CurrentMovementState ==
                           PlayerMovementState.EPlayerMovementState.Sprinting;
        bool isJumping = _playerMovementState.CurrentGroundState == PlayerMovementState.EPlayerGroundState.Jumping;
        bool isFalling = _playerMovementState.CurrentGroundState == PlayerMovementState.EPlayerGroundState.Falling;
        bool isGrounded = _playerMovementState.InGroundState();


        Vector2 inputTarget = isSprinting
            ? PlayerInputHandler.Instance.MovementInputValue * 1.5f
            : PlayerInputHandler.Instance.MovementInputValue;

        // Lerping the input values for smoother transitions
        _currentBlendInput = Vector2.Lerp(_currentBlendInput, inputTarget, _animationBlendSpeed * Time.deltaTime);

        // Set the animator parameters
        _animator.SetFloat(InputXHash, _currentBlendInput.x);
        _animator.SetFloat(InputYHash, _currentBlendInput.y);
        _animator.SetFloat(InputMagHash, _currentBlendInput.magnitude);
        _animator.SetBool(IsJumpingHash, isJumping);
        _animator.SetBool(IsFallingHash, isFalling);
        _animator.SetBool(IsGroundedHash, isGrounded);
    }
}
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimationHandler : MonoBehaviour
{
    [Header("Animation")] [SerializeField] private Animator _animator;
    [SerializeField] private float _animationBlendSpeed = 0.02f;
    [FormerlySerializedAs("_playerMovementState")] [SerializeField] private PlayerState _playerState;

    // So we don't have to use strings
    private static readonly int InputXHash = Animator.StringToHash("InputX");
    private static readonly int InputYHash = Animator.StringToHash("InputY");
    private static readonly int InputMagHash = Animator.StringToHash("InputMagnitude");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsLeftPunchingHash = Animator.StringToHash("IsLeftPunching");
    private static readonly int IsRightPunchingHash = Animator.StringToHash("IsRightPunching");
    private static readonly int IsPlayingActionHash = Animator.StringToHash("IsPlayingAction");

    private Vector2 _currentBlendInput = Vector2.zero;

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isSprinting = _playerState.CurrentMovementState ==
                           PlayerState.EPlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentGroundState == PlayerState.EPlayerGroundState.Jumping;
        bool isFalling = _playerState.CurrentGroundState == PlayerState.EPlayerGroundState.Falling;
        bool isGrounded = _playerState.InGroundState();
        
        bool isLeftPunching = _playerState.CurrentAttackState == PlayerState.EPlayerAttackState.LeftPunching;
        bool isRightPunching = _playerState.CurrentAttackState == PlayerState.EPlayerAttackState.RightPunching;
        bool isPlayingAction = _playerState.CurrentAttackState != PlayerState.EPlayerAttackState.Idle;


        Vector2 inputTarget = isSprinting
            ? PlayerInputHandler.Instance.MovementInputValue * 1.5f
            : PlayerInputHandler.Instance.MovementInputValue;

        // Lerping the input values for smoother transitions
        _currentBlendInput = Vector2.Lerp(_currentBlendInput, inputTarget, _animationBlendSpeed * Time.deltaTime);
        
        
        // set the movement anim states
        _animator.SetFloat(InputXHash, _currentBlendInput.x);
        _animator.SetFloat(InputYHash, _currentBlendInput.y);
        _animator.SetFloat(InputMagHash, _currentBlendInput.magnitude);
        
        // Set ground anim states
        _animator.SetBool(IsJumpingHash, isJumping);
        _animator.SetBool(IsFallingHash, isFalling);
        _animator.SetBool(IsGroundedHash, isGrounded);
        
        // Set attack anim states
        _animator.SetBool(IsLeftPunchingHash, isLeftPunching);
        _animator.SetBool(IsRightPunchingHash, isRightPunching);
        
        // Set action anim states
        _animator.SetBool(IsPlayingActionHash, isPlayingAction);
    }
}
using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NpcAnimationController : MonoBehaviour
{
    private Animator _animator;
    
    [Header("Animation")]
    [SerializeField] private AnimationClip placeholderClip; // e.g. empty clip
    private AnimatorOverrideController overrideController;

    [Header("Movement Settings")]
    [SerializeField] private float _animationBlendSpeed = 6f;

    // So we don't have to use strings
    private static readonly int MovementMagnitudeHash = Animator.StringToHash("MovementMagnitude");
    private static readonly int PlayActionHash = Animator.StringToHash("PlayAction");

    private float _movementMagnitudeBlend = 0f;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = overrideController;
    }

    public void UpdateAnimator(float normalizedMovementMagnitude)
    {
        // lerp the movement magnitude for smoother transitions
        _movementMagnitudeBlend = Mathf.Lerp(_movementMagnitudeBlend, normalizedMovementMagnitude,
            _animationBlendSpeed * Time.deltaTime);
        
        // Set the movement magnitude in the animator
        _animator.SetFloat(MovementMagnitudeHash, _movementMagnitudeBlend);
    }
    
    public void UpdateAnimationAction(AnimationClip actionClip)
    {
        if (actionClip)
        {
            overrideController["ActionPlaceholder"] = actionClip; // name of state’s original clip
            _animator.SetBool(PlayActionHash, true);
        }
        else // turn off the rest
        {
            _animator.SetBool(PlayActionHash, false);
            // overrideController["ActionPlaceholder"] = placeholderClip; // name of state’s original clip
        }
    }
}
using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NpcAnimationController : MonoBehaviour
{
    private Animator _animator;

    [Header("Movement Settings")]
    [SerializeField] private float _animationBlendSpeed = 6f;

    // So we don't have to use strings
    private static readonly int MovementMagnitudeHash = Animator.StringToHash("MovementMagnitude");

    private float _movementMagnitudeBlend = 0f;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimator(float normalizedMovementMagnitude)
    {
        // lerp the movement magnitude for smoother transitions
        _movementMagnitudeBlend = Mathf.Lerp(_movementMagnitudeBlend, normalizedMovementMagnitude,
            _animationBlendSpeed * Time.deltaTime);
        
        // Set the movement magnitude in the animator
        _animator.SetFloat(MovementMagnitudeHash, _movementMagnitudeBlend);
    }
}
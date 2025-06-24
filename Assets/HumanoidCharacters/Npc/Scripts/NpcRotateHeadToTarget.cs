using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NpcRotateHeadToTarget : MonoBehaviour
{
    [SerializeField] private Transform _headTransform;
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private string _targetTag;
    [SerializeField] private bool _applyAngleClamp = false;
    [SerializeField] private float _clampAngle = 90f;
    [SerializeField, ReadOnly] private HeadRotationState _headRotationState = HeadRotationState.None;
    
    private Transform _targetToLookAt;
    
    // enum to trach the state of the head rotation
    private enum HeadRotationState
    {
        None,
        ResetToDefault,
        FocusOnTarget,
    }

    // Update is called once per frame
    void Update()
    {
        // no target to look at
        if (_headRotationState == HeadRotationState.None) return;
        
        if (_headRotationState == HeadRotationState.FocusOnTarget) UpdateFocusOnTarget();
        else if (_headRotationState == HeadRotationState.ResetToDefault) UpdateResetToDefault();
    }

    private void UpdateResetToDefault()
    {
        Vector3 direction = _bodyTransform.forward;
        direction.y = 0; // Ignore vertical difference
        
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Apply offset around Y axis
        Vector3 eulerAngles = lookRotation.eulerAngles;
        eulerAngles.y += -90;
        Quaternion targetRotation = Quaternion.Euler(eulerAngles);
        
        // Lock the X and Z rotation
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = 0; // Lock X
        euler.z = -90; // Lock Z
        targetRotation = Quaternion.Euler(euler);
        

        // Apply the rotation smoothly
        _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation, targetRotation, Time.deltaTime * 5f);

        // Turn off the rotation updates if we reached the target rotation
        if (Quaternion.Angle(_headTransform.rotation, targetRotation) < 0.1f)
        {
            _headRotationState = HeadRotationState.None;
        }
    }

    private void UpdateFocusOnTarget()
    {
        // Calculate direction to target (ignoring vertical difference)
        Vector3 direction = _targetToLookAt.position - _headTransform.position;
        direction.y = 0; // Ignore vertical difference

        // Get rotation toward the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Apply offset around Y axis
        Vector3 eulerAngles = lookRotation.eulerAngles;
        eulerAngles.y += -90;
        Quaternion targetRotation = Quaternion.Euler(eulerAngles);
        
        // Optionally apply angle clamp
        if (_applyAngleClamp)
        {
            Vector3 bodyForward = _bodyTransform.forward;
            bodyForward.y = 0;
            Vector3 lookDir = lookRotation * Vector3.forward;
            lookDir.y = 0;

            // Calculate signed angle difference on Y axis
            float angle = Vector3.SignedAngle(bodyForward, lookDir, Vector3.up);

            // Clamp the angle
            float clampedAngle = Mathf.Clamp(angle, -_clampAngle, _clampAngle);
            
            // Calculate new target rotation angle based on body forward and clamped angle
            targetRotation = Quaternion.AngleAxis(clampedAngle, Vector3.up) * _bodyTransform.rotation;
            
            // reapply the offset around Y axis since it's been modified
            eulerAngles = targetRotation.eulerAngles;
            eulerAngles.y += -90;
            targetRotation = Quaternion.Euler(eulerAngles);
        }
        

        // Lock the X and Z rotation
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = 0; // Lock X
        euler.z = -90; // Lock Z
        targetRotation = Quaternion.Euler(euler);
        

        // Apply the rotation smoothly
        _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_targetTag)) return;
        if (!_headTransform) return;
        if (!_bodyTransform) return;
        
        _headRotationState = HeadRotationState.FocusOnTarget;
        _targetToLookAt = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_headRotationState != HeadRotationState.FocusOnTarget) return;
        if (!other.CompareTag(_targetTag)) return;
        
        _headRotationState = HeadRotationState.ResetToDefault;
        _targetToLookAt = null;
    }
}

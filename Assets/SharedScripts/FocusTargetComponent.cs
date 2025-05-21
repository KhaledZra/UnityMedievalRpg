using System;
using UnityEngine;

public class FocusTargetComponent : MonoBehaviour
{
    private Transform _targetTransform;

    [SerializeField] private Transform _currentTransform;
    [SerializeField] private string _targetTag;

    private bool _startFocus;

    void Update()
    {
        if (!_startFocus) return;
        if (_currentTransform && _targetTransform == null) return;
        
        // Update the rotation to face the target only on y axis
        Vector3 direction = _targetTransform.position - _currentTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation.x = _currentTransform.rotation.x; // Lock x rotation
        targetRotation.z = _currentTransform.rotation.z; // Lock z rotation
        _currentTransform.rotation = Quaternion.Slerp(_currentTransform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag))
        {
            _startFocus = true;
            _targetTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _startFocus = false;
    }
}
using UnityEngine;

public class FocusTargetComponent : MonoBehaviour
{
    private Transform _targetTransform;

    [SerializeField] private Transform _currentTransform;
    [SerializeField] private string _targetTag;
    [SerializeField] private float _yAxisOffset = 0f;
    private Quaternion _baseOffset;

    private bool _startFocus;
    
    void Start()
    {
        _baseOffset = Quaternion.Euler(0, _yAxisOffset, 0);
    }

    void Update()
    {
        UpdateFocusRotation();
    }

    private void UpdateFocusRotation()
    {
        if (!_startFocus) return;
        
        // Calculate direction to target (ignoring vertical difference)
        Vector3 direction = _targetTransform.position - _currentTransform.position;
        direction.y = 0f; // Ensure only Y-axis rotation

        // Get rotation toward the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Apply offset around Y
        Quaternion targetRotation = lookRotation * _baseOffset;

        // Smoothly rotate only around Y
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = _currentTransform.eulerAngles.x; // Lock X
        euler.z = _currentTransform.eulerAngles.z; // Lock Z
        targetRotation = Quaternion.Euler(euler);

        // Apply the rotation smoothly
        _currentTransform.rotation = Quaternion.Slerp(_currentTransform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_targetTag)) return;
        if (!_currentTransform) return;
        
        _startFocus = true;
        _targetTransform = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_startFocus) return;
        if (!other.CompareTag(_targetTag)) return;
        
        _startFocus = false;
        _targetTransform = null;
    }
}
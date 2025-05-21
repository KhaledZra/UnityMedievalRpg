using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private GameObject _parent;
    [SerializeField] private TextMeshPro _textMeshPro;
    [SerializeField] private Animator _animator;
    
    private Transform _targetTransform;
    private Vector3 _targetPosition;
    
    private void Awake()
    {
        if (_animator == null) return;

        // Get the original clip from the animator
        AnimationClip originalClip = _animator.runtimeAnimatorController.animationClips[0];

        // Create a unique instance of the original clip
        AnimationClip uniqueClip = Instantiate(originalClip);
        uniqueClip.name = originalClip.name + "_Unique";

        // Get the curve binding for m_AnchoredPosition.x
        var curveBinding = AnimationUtility.GetCurveBindings(uniqueClip)
            .FirstOrDefault(binding => binding.propertyName == "m_AnchoredPosition.x");

        // Modify the last keyframe of the curve to randomize the value
        AnimationCurve curve = AnimationUtility.GetEditorCurve(uniqueClip, curveBinding);
        if (curve != null && curve.length > 0)
        {
            Keyframe lastKey = curve[curve.length - 1];
            lastKey.value = Random.Range(-2f, 2f); // Randomize the last value
            curve.MoveKey(curve.length - 1, lastKey);
            uniqueClip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", curve);
        }

        // Replace original clip with the unique one
        AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        overrideController[originalClip.name] = uniqueClip;

        // Apply new override controller
        _animator.runtimeAnimatorController = overrideController;
    }

    public void Update()
    {
        if (_targetTransform)
        {
            _parent.transform.position = _targetTransform.position + new Vector3(0, 1.25f, 0);
        }
        else if (_targetPosition != Vector3.zero)
        {
            _parent.transform.position = _targetPosition + new Vector3(0, 1.25f, 0);
        }
    }

    public void Initialize(int damage, Color color, Transform targetTransform)
    {
        _textMeshPro.text = damage.ToString();
        _textMeshPro.color = color;
        _targetTransform = targetTransform;
    }
    
    public void InitializePosition(int damage, Color color, Vector3 targetPosition)
    {
        _textMeshPro.text = damage.ToString();
        _textMeshPro.color = color;
        _targetPosition = targetPosition;
    }

    public void OnAnimationEnd()
    {
        Destroy(_parent ? _parent : gameObject);
    }
}
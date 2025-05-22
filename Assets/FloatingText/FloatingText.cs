using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private GameObject _parent;
    [SerializeField] private TextMeshPro _textMeshPro;
    [SerializeField] private Animator _animator;
    
    private Vector3 _targetPosition;
    
    private void Awake()
    {
        if (_animator == null) return;
        if (_animator.runtimeAnimatorController.animationClips.Length == 0) return;
        
        // Get the original clip from the animator
        AnimationClip originalClip = _animator.runtimeAnimatorController.animationClips[0];

        // Create a unique instance of the original clip
        AnimationClip uniqueClip = Instantiate(originalClip);
        uniqueClip.name = originalClip.name + "_Unique";
        
        // Fine, I'll do it myself - Thanos
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(2f, Random.Range(-2f, 2f)) // All of that for a drop of blood - Thanos
        );
        
        uniqueClip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", curve);

        // Replace original clip with the unique one
        AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        overrideController[originalClip.name] = uniqueClip;

        // Apply new override controller
        _animator.runtimeAnimatorController = overrideController;
    }
    
    public void Initialize(int damage, Color color, Vector3 targetPosition)
    {
        _textMeshPro.text = damage.ToString();
        _textMeshPro.color = color;
        _targetPosition = targetPosition;
        _parent.transform.position = _targetPosition + new Vector3(0, 1.25f, 0);
    }

    public void OnAnimationEnd()
    {
        Destroy(_parent ? _parent : gameObject);
    }
}
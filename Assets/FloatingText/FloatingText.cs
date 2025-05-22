using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private GameObject _parent;
    [SerializeField] private TextMeshPro _textMeshPro;
    [SerializeField] private Animation _animation;
    
    private Vector3 _targetPosition;
    
    private void Start()
    {
        if (_animation == null) return;
        if (_animation.clip == null) return;
        
        // Get the original clip from the animator
        AnimationClip originalClip = _animation.clip;

        // Create a unique instance of the original clip
        AnimationClip uniqueClip = Instantiate(originalClip);
        uniqueClip.name = originalClip.name + "_Unique";
        
        // Fine, I'll do it myself - Thanos
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(2f, Random.Range(-2f, 2f)) // All of that for a drop of blood - Thanos
        );
        
        uniqueClip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", curve);
        
        _animation.RemoveClip(originalClip);
        _animation.AddClip(uniqueClip, uniqueClip.name);
        _animation.Play(uniqueClip.name);
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
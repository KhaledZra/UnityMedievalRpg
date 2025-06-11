using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PatrolPointActionValues", menuName = "Scriptable Objects/PatrolPointActionValues")]
public class PatrolPointActionValues : ScriptableObject
{
    public AnimationClip actionClip; // Animation clip to play at this patrol point
    public float actionDuration; // Duration of the action at this patrol point
    
    // Saved values
    [ReadOnly, SerializeField] private AnimationClip _defaultActionClip;
    [ReadOnly, SerializeField] private float _defaultActionDuration = 5f;
    
    private void OnEnable()
    {
        Reset();
    }

    [Button]
    private void Reset()
    {
        // Set default values for the action state
        actionClip = _defaultActionClip;
        actionDuration = _defaultActionDuration;
    }
    
    [Button]
    private void Save()
    {
        // Save the current values to the scriptable object
        _defaultActionClip = actionClip;
        _defaultActionDuration = actionDuration;
    }
}

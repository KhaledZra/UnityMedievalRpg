using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PatrolPointActionValues", menuName = "Scriptable Objects/PatrolPointActionValues")]
public class PatrolPointActionValues : ScriptableObject
{
    public AnimationClip actionClip; // Animation clip to play at this patrol point
    public float actionDuration; // Duration of the action at this patrol point
    public float actionCompletionDelay; // Delay after the action is completed 
    
    // Saved values
    [ReadOnly, SerializeField] private AnimationClip _defaultActionClip;
    [ReadOnly, SerializeField] private float _defaultActionDuration = 5f;
    [ReadOnly, SerializeField] private float _defaultActionCompletionDelay = 0.5f;
    
    
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
        actionCompletionDelay = _defaultActionCompletionDelay;
    }
    
    [Button]
    private void Save()
    {
        // Save the current values to the scriptable object
        _defaultActionClip = actionClip;
        _defaultActionDuration = actionDuration;
        _defaultActionCompletionDelay = actionCompletionDelay;
    }
}

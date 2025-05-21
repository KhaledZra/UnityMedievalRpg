using UnityEngine;

public class FloatingTextHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _floatingTextPrefab;

    public void SpawnFloatingText(int damage, Color color, Transform targetTransform)
    {
        if (_floatingTextPrefab == null)
        {
            Debug.LogError("Floating text prefab is not assigned.");
            return;
        }
        
        GameObject floatingText = Instantiate(_floatingTextPrefab);
        FloatingText textComponent = floatingText.GetComponentInChildren<FloatingText>();
        textComponent?.Initialize(damage, color, targetTransform);
    }
    
    public void SpawnFloatingTextPosition(int damage, Color color, Vector3 targetPosition)
    {
        if (_floatingTextPrefab == null)
        {
            Debug.LogError("Floating text prefab is not assigned.");
            return;
        }
        
        GameObject floatingText = Instantiate(_floatingTextPrefab);
        FloatingText textComponent = floatingText.GetComponentInChildren<FloatingText>();
        textComponent?.InitializePosition(damage, color, targetPosition);
    }
}

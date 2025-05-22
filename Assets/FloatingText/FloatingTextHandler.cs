using UnityEngine;

public class FloatingTextHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _floatingTextPrefab;
    
    public void SpawnFloatingTextAtPosition(int damage, Color color, Vector3 targetPosition, Quaternion rotation)
    {
        if (_floatingTextPrefab == null)
        {
            Debug.LogError("Floating text prefab is not assigned.");
            return;
        }
        
        GameObject floatingText = Instantiate(_floatingTextPrefab, targetPosition, rotation);
        FloatingText textComponent = floatingText.GetComponentInChildren<FloatingText>();
        textComponent?.Initialize(damage, color, targetPosition);
    }
}

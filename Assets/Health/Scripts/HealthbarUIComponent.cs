using UnityEngine;
using UnityEngine.UI;

public class HealthbarUIComponent : MonoBehaviour
{
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private Transform _followTarget;

    private void Update()
    {
        // Update to follow the target
        transform.position = _followTarget.position + new Vector3(0, 1, 0);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthBarImage == null)
        {
            Debug.LogError("Health bar image is not assigned.");
            return;
        }
        
        float healthPercentage = currentHealth / maxHealth;
        _healthBarImage.fillAmount = healthPercentage;
    }
    
    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}

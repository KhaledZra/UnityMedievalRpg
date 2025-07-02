using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _interactionPrompt;

    private void Start()
    {
        ShowInteractionPrompt(false);
    }

    public void ShowInteractionPrompt(bool visible)
    {
        _interactionPrompt.SetActive(visible);
    }
}

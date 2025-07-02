using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TalkUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _talkUICanvas;
    [SerializeField] private Button _stopInteractionButton;

    private void Start()
    {
        if (_talkUICanvas)
        {
            _talkUICanvas.SetActive(false);
        }
    }

    public void SetInteractor(UnityAction stopInteraction)
    {
        // Add listener to the stop interaction button
        _stopInteractionButton.onClick.AddListener(stopInteraction);
    }
    
    public void SetTalkUIVisibility(bool show)
    {
        // Enable or disable the stop interaction button
        if (_talkUICanvas)
        {
            _talkUICanvas.SetActive(show);
        }
    }
}

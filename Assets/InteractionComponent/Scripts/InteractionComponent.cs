using System;
using System.Collections;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactionLayerMask;
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private PlayerUIManager _playerUIManager;

    private IInteractable _interactable;
    private bool _interactionEnabled = true;

    private void Start()
    {
        StartCoroutine(UpdateInteraction());
    }

    private void OnDrawGizmos()
    {
        if (_camera == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(_camera.transform.position, _camera.transform.forward * _interactionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_camera.transform.position + _camera.transform.forward * _interactionDistance, 0.5f);
    }

    private IEnumerator UpdateInteraction()
    {
        while (true)
        {
            if (_interactionEnabled)
            {
                _interactable = null;
                Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
                
                // todo: remove after debugging
                Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 10f, Color.yellow, 10f);

                if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
                {
                    // Checks if the hit point is not behind the interactor
                    if (gameObject.transform.InverseTransformPoint(hit.point).z >= 0)
                    {
                        // Check if the object has an interaction component & store it
                        if (!hit.collider.TryGetComponent(out _interactable))
                        {
                            // Reset the interactable and hide the prompt
                            _playerUIManager?.ShowInteractionPrompt(false);
                        }
                        else
                        {
                            // Show the interaction prompt
                            _playerUIManager?.ShowInteractionPrompt(true);
                        }
                    }
                    else
                    {
                        // Reset the interactable and hide the prompt
                        _playerUIManager?.ShowInteractionPrompt(false);
                    }
                }
                else
                {
                    // Reset the interactable and hide the prompt
                    _playerUIManager?.ShowInteractionPrompt(false);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnInteract()
    {
        _interactable?.Interact(gameObject);
    }

    public void ToggleInteraction(bool turnOn)
    {
        if (_playerUIManager)
        {
            _playerUIManager?.ShowInteractionPrompt(false);
        }

        _interactionEnabled = turnOn;
    }
}
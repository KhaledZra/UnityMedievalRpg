using System;
using System.Collections;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactionLayerMask;
    [SerializeField] private float _interactionDistance = 3f;
    
    private IInteractable _interactable;

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
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
            {
                // Check if the object has an interaction component & store it
                if (hit.collider.TryGetComponent(out _interactable)) { }
            }
        
            yield return new WaitForSeconds(1f);
        }
    }

    public void Interact()
    {
        _interactable?.Interact();
    }
}
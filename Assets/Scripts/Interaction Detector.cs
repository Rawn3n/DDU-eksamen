using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public IInteractable interactableInRange = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
            interactableInRange = interactable;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
            interactableInRange = null;
    }
}
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public InteractionDetector interactionDetector;
    public GameObject dashIcon;
    public GameObject interactIcon;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);
    public float fadeSpeed = 12f;

    private SpriteRenderer dashRenderer;
    private SpriteRenderer interactRenderer;

    void Start()
    {
        dashRenderer = dashIcon.GetComponent<SpriteRenderer>();
        interactRenderer = interactIcon.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        transform.position = playerController.transform.position + offset;

        bool showInteract = interactionDetector != null && interactionDetector.interactableInRange != null;
        bool showDash = playerController.canDash && !showInteract;

        FadeIcon(dashRenderer, showDash);
        FadeIcon(interactRenderer, showInteract);
    }

    void FadeIcon(SpriteRenderer icon, bool visible)
    {
        if (icon == null) return;
        Color c = icon.color;
        c.a = Mathf.Lerp(c.a, visible ? 1f : 0f, Time.deltaTime * fadeSpeed);
        icon.color = c;
    }
}
using UnityEngine;
using TMPro;

public class DashIndicator : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public GameObject Canvas;
    public Camera mainCamera;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);  // Offset
    //public string label = "R2";

    private TextMeshProUGUI buttonText;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        buttonText = Canvas.GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = Canvas.GetComponent<CanvasGroup>();

        if (buttonText != null)
        {
            //buttonText.text = label;
        }

        canvasGroup.blocksRaycasts = false; // ikke bloker raycast (det skal ikke have indflydelse på spillet)
    }

    void LateUpdate()
    {
        // Følg efter spilleren
        Canvas.transform.position = playerController.transform.position + offset;

        // Altid se mod kameraet
        Canvas.transform.LookAt(Canvas.transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

        // Læs canDash fra PlayerController
        SetVisible(playerController.canDash);
    }

    void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            // Fade ind og ud
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, visible ? 1f : 0f, Time.deltaTime * 12f);
            canvasGroup.interactable = visible;
        }
        else
        {
            Canvas.SetActive(visible);
        }
    }
}
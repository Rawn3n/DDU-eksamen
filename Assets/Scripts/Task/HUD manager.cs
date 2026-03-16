using UnityEngine;
using TMPro;
using System.Collections;

public class HUD_manager : MonoBehaviour
{
    public static HUD_manager Instance;

    [Header("Popup Effekt")]
    [SerializeField] private TMP_Text popupText;

    private void Awake()
    {
        Instance = this;
        popupText.color = new Color(1, 1, 1, 0);
        popupText.transform.localScale = Vector3.zero;
    }
    public void VisPopup(string besked)
    {
        StopAllCoroutines();
        StartCoroutine(PopupAnimation(besked));
    }

    private IEnumerator PopupAnimation(string besked)
    {
        popupText.text = besked;

        // Pop ind
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            popupText.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.2f, elapsed / 0.2f);
            popupText.color = new Color(1, 1, 1, 1);
            yield return null;
        }

        // Bounce
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            popupText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, elapsed / 0.1f);
            yield return null;
        }

        popupText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.5f);

        // Fade ud
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            popupText.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, elapsed / 0.5f));
            yield return null;
        }

        popupText.color = new Color(1, 1, 1, 0);
        popupText.transform.localScale = Vector3.zero;
    }

    // ---- TILFØJ FLERE HER EFTERHÅNDEN ----
    // public void UpdateHealth(int current, int max) { }
    // public void UpdateTimer(float sekunder) { }
}
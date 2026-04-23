using UnityEngine;
using System.Collections;

public class Lektier : Task
{
    public static int total = 6;
    private static int samlede = 0;
    [SerializeField] private bool depsawnEffect = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (depsawnEffect == true)
            {
                Destroy(gameObject);
            }
            else LektieCollected();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (depsawnEffect == true)
            {
                StartCoroutine(FadeEffect());
            }
            else return;
        }
    }


    private void LektieCollected()
    {
        samlede++;
        HUD_manager.Instance.VisPopup($"{samlede}/{total}");

        if (samlede >= total)
        {
            CompleteTask();
        }

        Destroy(gameObject);
    }

    IEnumerator FadeEffect()
        {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        float fadeDuration = 0.5f; // hastighed af fade effect i sekunder
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}
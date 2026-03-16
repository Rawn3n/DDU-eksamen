// Lektier.cs - sæt på hvert lektie objekt
using UnityEngine;

public class Lektier : Task
{
    public static int total = 6;
    private static int samlede = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LektieCollected();
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
}
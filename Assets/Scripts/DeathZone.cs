using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem h = collision.gameObject.GetComponent<HealthSystem>();
            if (h != null)
            {
                h.Die();
            }
        }
    }
}

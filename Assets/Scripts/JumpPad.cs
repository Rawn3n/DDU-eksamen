using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpForce = 500f;
    public AudioClip jumpLyd;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(new Vector2(0, jumpForce));

                if (audioSource != null && jumpLyd != null)
                {
                    audioSource.PlayOneShot(jumpLyd);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}

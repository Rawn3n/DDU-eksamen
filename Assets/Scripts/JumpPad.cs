using TMPro;
using UnityEngine;
using System.Collections;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpForce = 500f;
    public AudioClip jumpLyd;
    private AudioSource audioSource;
    private Animator animator;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(new Vector2(0, jumpForce));

                //if (audioSource != null && jumpLyd != null)
                //{
                //    audioSource.PlayOneShot(jumpLyd);
                //}


                if (animator != null)   
                {
                    //animator.SetBool("isActiveted", true);
                    //Debug.Log("jump");
                    StopAllCoroutines();
                    StartCoroutine(Animation(1f));
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (animator != null)
        //{
        //    StartCoroutine(Delay(1f));
        //    animator.SetBool("isActiveted", false);

        //}

    }

    private IEnumerator Animation(float f)
    {
        animator.SetBool("isActiveted", true);
        Debug.Log("jump");
        if (audioSource != null && jumpLyd != null)
        {
            audioSource.PlayOneShot(jumpLyd);
        }
        yield return new WaitForSeconds(f);
        animator.SetBool("isActiveted", false);
    }
}

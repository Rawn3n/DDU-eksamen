using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    int currentHealth;
    public AudioClip deathLyd;
    private AudioSource audioSource;
    private Respawn respawn;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        respawn = FindFirstObjectByType<Respawn>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Debug.Log("DØD!");

        if (audioSource != null && deathLyd != null)
        {
            audioSource.PlayOneShot(deathLyd);
        }

        respawn.RespawnPlayer(gameObject);
    }
}

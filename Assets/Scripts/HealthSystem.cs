using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] int maxHealth = 1;
    int currentHealth;
    public AudioClip deathLyd;
    private AudioSource audioSource;
    public Vector3 respawnPoint;
    public GameObject respawnEffectPrefab;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position; // sætter spawnpoint til start postion
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        Debug.Log("DØD!");
        if (audioSource != null && deathLyd != null)
        {
            audioSource.PlayOneShot(deathLyd);
        }

        transform.position = respawnPoint; //Repsawn
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, respawnPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public void SetSpawnPoint(Vector3 newSpawn, GameObject player)
    {
        HealthSystem h = player.GetComponent<HealthSystem>();
        if (h != null)
        {
            h.respawnPoint = newSpawn;
            Debug.Log($"New spawn point set for {player.name} at: {newSpawn}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetSpawnPoint(transform.position, collision.gameObject);
        }
    }
}
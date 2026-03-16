using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Vector3 spawnPoint;

    private void Start()
    {
        spawnPoint = transform.position;
    }

    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = spawnPoint;
    }

    public void SetSpawnPoint(Vector3 newSpawn)
    {
        Debug.Log($"New spawn point set at: {newSpawn}");
        spawnPoint = newSpawn;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetSpawnPoint(collision.gameObject.transform.position);
        }
    }
}
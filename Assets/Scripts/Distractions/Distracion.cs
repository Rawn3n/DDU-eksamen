using UnityEngine;

public class Distraction : MonoBehaviour
{
    [Header("Noter")]
    public GameObject musicNotePrefab;
    public int noteCount = 8;
    public float noteSpeed = 4f;
    public float noteSpawnSpread = 2f;

    [Header("Spawn punkt")]
    public Transform spawnPoint;
    public bool spawnFromLeft = true; // Retning noterne bevæger sig

    private void Start()
    {
        Invoke(nameof(TriggerDistraction), 5f); //5 sekunder delay
    }

    public void TriggerDistraction()
    {
        SpawnNotes();
    }

    private void SpawnNotes()
    {
        Camera cam = Camera.main;
        float worldZ = Mathf.Abs(cam.transform.position.z);

        Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, worldZ));
        Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, worldZ));

        float spawnX = spawnFromLeft ? leftEdge.x - 1f : rightEdge.x + 1f;
        float centerY = cam.transform.position.y;

        Vector2 moveDir = spawnFromLeft ? Vector2.right : Vector2.left;
        float push = spawnFromLeft ? 1f : -1f;

        for (int i = 0; i < noteCount; i++)
        {
            float offsetY = Random.Range(-noteSpawnSpread, noteSpawnSpread);
            Vector3 spawnPos = new Vector3(spawnX, centerY + offsetY, 0f);

            GameObject note = Instantiate(musicNotePrefab, spawnPos, Quaternion.identity);
            note.GetComponent<MusicNote>().Init(moveDir, noteSpeed + Random.Range(-0.5f, 0.5f), push);
        }
    }
}
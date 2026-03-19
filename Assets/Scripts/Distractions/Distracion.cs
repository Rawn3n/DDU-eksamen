using UnityEngine;
using System.Collections;

public class Distraction : MonoBehaviour
{
    [Header("Notes")]
    public GameObject musicNotePrefab;
    public int noteCount = 8;
    public float noteSpeed = 4f;
    public float noteSpawnSpread = 2f;

    [Header("Distraction")]
    public float distractionDuration = 4f;

    private bool isDistracted = false;
    private float autoWalkDirection = 1f;
    private PlayerController[] players;

    private void Start()
    {
        players = new PlayerController[] { GameManager.Instance.player1, GameManager.Instance.player2 };
        StartCoroutine(TriggerDistraction());
    }


    public IEnumerator TriggerDistraction() // Kan blive kaldt alle steder ved brug af StartCoroutine(TriggerDistraction());
    {
        isDistracted = true;
        bool fromLeft = Random.value > 0.5f;
        autoWalkDirection = fromLeft ? 1f : -1f;
        SpawnNotes(fromLeft, autoWalkDirection);

        float timer = 0f;
        while (timer < distractionDuration)
        {
            PlayerController activePlayer = players[GameManager.Instance.activePlayerIndex];
            activePlayer.distractionInput = new Vector2(autoWalkDirection, 0f);
            timer += Time.deltaTime;
            yield return null;
        }
        isDistracted = false;
        foreach (PlayerController player in players)
        {
            player.distractionInput = Vector2.zero;
        }
    }

    private void SpawnNotes(bool fromLeft, float pushDirection)
    {
        Camera cam = Camera.main;

        float worldZ = Mathf.Abs(cam.transform.position.z);

        Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, worldZ));
        Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, worldZ));

        float spawnX = fromLeft ? leftEdge.x - 1f : rightEdge.x + 1f;
        float centerY = cam.transform.position.y;
        Vector2 moveDir = fromLeft ? Vector2.right : Vector2.left;

        for (int i = 0; i < noteCount; i++)
        {
            float spawnY = centerY + Random.Range(-noteSpawnSpread, noteSpawnSpread);
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject note = Instantiate(musicNotePrefab, spawnPos, Quaternion.identity);
            note.GetComponent<MusicNote>().Init(moveDir, noteSpeed + Random.Range(-0.5f, 0.5f), pushDirection, players);
        }
    }
}
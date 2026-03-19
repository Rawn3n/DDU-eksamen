using UnityEngine;

public class MusicNote : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float pushDirection;
    private PlayerController[] players;
    private bool isVisible = false;

    public void Init(Vector2 moveDirection, float moveSpeed, float push, PlayerController[] playerRefs)
    {
        direction = moveDirection;
        speed = moveSpeed;
        pushDirection = push;
        players = playerRefs;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // gør så at spilleren kun rykker sig når musik viser sig
        if (isVisible)
        {
            PlayerController activePlayer = players[GameManager.Instance.activePlayerIndex];
            activePlayer.distractionInput = new Vector2(pushDirection, 0f);
        }
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        isVisible = false;

        if (players != null)
        {
            foreach (PlayerController player in players)
            {
                player.distractionInput = Vector2.zero;
            }
        }
        Destroy(gameObject);
    }
}
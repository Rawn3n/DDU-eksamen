using UnityEngine;

public class MusicNote : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float pushStrength = 1f;

    private bool active = false;

    public void Init(Vector2 moveDirection, float moveSpeed, float push)
    {
        direction = moveDirection;
        speed = moveSpeed;
        pushStrength = push;

        // Vent et frame før vi lytter på OnBecameInvisible, så noter der spawner udenfor kameraet ikke dør med det samme
        Invoke(nameof(Activate), 2f);
    }

    private void Activate() => active = true;

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        PlayerController activePlayer = GameManager.Instance.GetActivePlayer();
        if (activePlayer != null)
        {
            activePlayer.distractionInput = new Vector2(pushStrength, 0f);
        }
    }

    private void OnBecameInvisible()
    {
        if (!active) return;

        PlayerController activePlayer = GameManager.Instance.GetActivePlayer();
        if (activePlayer != null)
        {
            activePlayer.distractionInput = Vector2.zero;
        }

        if (FindObjectsByType<MusicNote>(FindObjectsSortMode.None).Length <= 1)
        {
            Distraction.Instance.StopMusic();
        }

        Destroy(gameObject);
    }
}
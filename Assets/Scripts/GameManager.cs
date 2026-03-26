using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("Switch Settings")]
    [SerializeField] public float switchInterval = 10f;

    [Header("Switch Effect")]
    public GameObject switchEffectPrefab;

    public int activePlayerIndex = 0;

    private PlayerController ActivePlayer => activePlayerIndex == 0 ? player1 : player2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Kan blive kaldt ved GameManager.Instance.funktionnavn(); @Mikkel
    void Start()
    {
        SetActivePlayer(0);
        StartCoroutine(AutoSwitchLoop());
    }

    IEnumerator AutoSwitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval + Random.Range(-5f,5f));
            PlayerSwitch();
        }
    }

    public void PlayerSwitch()
    {
        PlayerController leaving = activePlayerIndex == 0 ? player1 : player2; //clear distraction
        leaving.distractionInput = Vector2.zero;

        activePlayerIndex = activePlayerIndex == 0 ? 1 : 0;
        SetActivePlayer(activePlayerIndex);
    }

    private void SetActivePlayer(int index)
    {
        player1.SetActive(index == 0);
        player2.SetActive(index == 1);

        Transform target = index == 0 ? player1.transform : player2.transform;
        CameraFollow.Instance.SetTarget(target);

        if (switchEffectPrefab != null) 
        {
            GameObject effect = Instantiate(switchEffectPrefab, target.position, Quaternion.identity);
            Destroy(effect, 2f);
        }


        Debug.Log("Now controlling: Player " + (index + 1));
    }

    public PlayerController GetActivePlayer()
    {
        return ActivePlayer;
    }

    #region Player Input Ting
    public void OnMove(InputAction.CallbackContext ctx)
    {
        ActivePlayer.ReceiveMove(ctx.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        ActivePlayer.ReceiveLook(ctx.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        ActivePlayer.ReceiveJump();
    }
    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        ActivePlayer.ReceiveDash();
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ActivePlayer.ReceiveSprint(true);
        else if (ctx.canceled) ActivePlayer.ReceiveSprint(false);
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ActivePlayer.ReceiveCrouch(true);
        else if (ctx.canceled) ActivePlayer.ReceiveCrouch(false);
    }

    public void OnWallSlide(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ActivePlayer.ReceiveWallSlide(true);
        else if (ctx.canceled) ActivePlayer.ReceiveWallSlide(false);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        ActivePlayer.ReceiveInteract();
    }
    #endregion
}
using UnityEngine;

public class InitLevel : MonoBehaviour
{
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;

    [SerializeField] private TMPro.TextMeshProUGUI popUpText;

    private void Awake()
    {
        LevelLoaded();
    }

    public bool LevelLoaded()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player prefabs not assigned in InitLevel.");
            return false;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return false;
        }
        if (popUpText == null)
        {
            Debug.LogError("PopUpText not assigned in InitLevel.");
        }

        GameManager.Instance.player1 = player1;
        GameManager.Instance.player2 = player2;
        HUD_manager.Instance.popupText = popUpText;
        Distraction.Instance.Init();
        GameManager.Instance.Init();

        return true;
    }
}

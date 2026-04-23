using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject pauseMenuUI; // ikke brugt rn
    public bool IsPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
    }


    public void FreezeGameplay()
    {
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void UnfreezeGameplay()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}
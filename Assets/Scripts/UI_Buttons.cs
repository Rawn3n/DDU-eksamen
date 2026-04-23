using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Buttons : MonoBehaviour
{
    public void LevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void StartLevel(int i)
    {
        SceneManager.LoadScene($"Level_{i}");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

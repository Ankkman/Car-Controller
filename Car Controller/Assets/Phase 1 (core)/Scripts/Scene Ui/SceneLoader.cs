using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadCarSelectionScene()
    {
        SceneManager.LoadScene("CarSelectScene"); 
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game_scene_mobile"); 
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

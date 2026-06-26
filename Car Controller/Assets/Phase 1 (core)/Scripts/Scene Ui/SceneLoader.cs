using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Load the Game Scene (Your mobile car scene)
    public void LoadGameScene()
    {
        // "GameScene" must be the exact name of your mobile car scene file
        SceneManager.LoadScene("Game_scene_mobile"); 
    }

    // Load the Main Menu Scene
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Quit the application
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
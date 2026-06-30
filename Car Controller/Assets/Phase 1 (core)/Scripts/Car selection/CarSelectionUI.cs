using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelectionUI : MonoBehaviour
{
    public void SelectCarAndStart(int carIndex)
    {
        // 1. Pull transmission modes and layouts out of memory storage
        SettingsManager.LoadSettings();
        
        // 2. Commit the newly chosen vehicle index alongside previous choices
        SettingsManager.SaveSettings(SettingsManager.IsAutomatic, SettingsManager.CurrentControl, carIndex);
        
        // 3. Move the player directly into your main driving scene match
        SceneManager.LoadScene("Game_scene_mobile");
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

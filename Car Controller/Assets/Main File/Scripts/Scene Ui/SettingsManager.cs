using UnityEngine;

public static class SettingsManager
{
    public static bool IsAutomatic = true;

    // Call this to load the saved setting
    public static void LoadSettings()
    {
        // Default to 1 (true) if no setting is found yet
        IsAutomatic = PlayerPrefs.GetInt("IsAutomatic", 1) == 1;
    }

    // Call this when the user changes the Toggle in the UI
    public static void SaveSettings(bool isAuto)
    {
        IsAutomatic = isAuto;
        PlayerPrefs.SetInt("IsAutomatic", isAuto ? 1 : 0);
        PlayerPrefs.Save(); // Forces it to write to disk immediately
    }
}
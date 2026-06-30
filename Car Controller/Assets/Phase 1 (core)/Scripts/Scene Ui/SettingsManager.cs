using UnityEngine;

public static class SettingsManager
{
    public enum ControlType { Buttons, Steering }
    
    public static bool IsAutomatic = true;
    public static ControlType CurrentControl = ControlType.Buttons;
    public static int SelectedCarIndex = 0; // Tracks chosen car

    public static void LoadSettings()
    {
        IsAutomatic = PlayerPrefs.GetInt("IsAutomatic", 1) == 1;
        CurrentControl = (ControlType)PlayerPrefs.GetInt("ControlType", 0);
        SelectedCarIndex = PlayerPrefs.GetInt("SelectedCar", 0); // Default to first car
    }

    public static void SaveSettings(bool isAuto, ControlType control, int carIndex)
    {
        IsAutomatic = isAuto;
        CurrentControl = control;
        SelectedCarIndex = carIndex;
        
        PlayerPrefs.SetInt("IsAutomatic", isAuto ? 1 : 0);
        PlayerPrefs.SetInt("ControlType", (int)control);
        PlayerPrefs.SetInt("SelectedCar", carIndex);
        PlayerPrefs.Save();
    }
}

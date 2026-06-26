using UnityEngine;

public static class SettingsManager
{
    public enum ControlType { Buttons, Steering }
    
    public static bool IsAutomatic = true;
    public static ControlType CurrentControl = ControlType.Buttons;

    public static void LoadSettings()
    {
        IsAutomatic = PlayerPrefs.GetInt("IsAutomatic", 1) == 1;
        CurrentControl = (ControlType)PlayerPrefs.GetInt("ControlType", 0); // 0 = Buttons, 1 = Steering
    }

    public static void SaveSettings(bool isAuto, ControlType control)
    {
        IsAutomatic = isAuto;
        CurrentControl = control;
        
        PlayerPrefs.SetInt("IsAutomatic", isAuto ? 1 : 0);
        PlayerPrefs.SetInt("ControlType", (int)control);
        PlayerPrefs.Save();
    }
}
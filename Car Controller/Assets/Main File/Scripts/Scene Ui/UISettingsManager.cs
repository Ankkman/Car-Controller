using UnityEngine;
using UnityEngine.UI;

public class UISettingsManager : MonoBehaviour
{
    public Toggle autoToggle;

    // We use OnEnable instead of Start. 
    // OnEnable runs every time this GameObject is turned ON (opened).
    void OnEnable()
    {
        SettingsManager.LoadSettings();
        
        // IMPORTANT: We remove the listener first so it doesn't trigger while we set the value!
        autoToggle.onValueChanged.RemoveListener(OnToggleChanged);
        
        // Set the toggle to match the saved setting
        autoToggle.isOn = SettingsManager.IsAutomatic;
        
        // Add the listener back so it listens for future clicks
        autoToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool isOn)
    {
        // Save the setting immediately when clicked
        SettingsManager.SaveSettings(isOn);
        
        // If the car scene is currently loaded, update the car immediately!
        GameObject car = GameObject.FindGameObjectWithTag("Player"); 
        if(car != null)
        {
            VehicleInputHandler handler = car.GetComponent<VehicleInputHandler>();
            if(handler != null) 
            {
                handler.SetTransmissionMode(isOn);
                Debug.Log("Car transmission updated to: " + (isOn ? "Auto" : "Manual"));
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISettingsManager : MonoBehaviour
{
    [Header("Auto Mode (Slide Toggle Assets)")]
    public Button autoModePillButton;      // The main pill background button for Auto Mode
    public Image autoModeStatusIndicator;  // The small indicator circle/pill inside the button
    public TMP_Text autoModeStatusText;    // Text component showing "ON" or "OFF"

    [Header("Control Layout Selection (Split Option Buttons)")]
    public Button buttonsLayoutOption;     // The "Buttons" selector button block
    public Button steeringLayoutOption;    // The "Steering Wheel" selector button block

    [Header("Design Aesthetics & Branding Color Schemes")]
    public Color colorSelected = new Color(1f, 1f, 1f, 1f);        // Sharp crisp solid white
    public Color colorUnselected = new Color(0.15f, 0.17f, 0.22f, 0.85f); // Deep slate unlit background dark gray
    public Color textSelectedColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark contrast text for selected buttons
    public Color textUnselectedColor = Color.white;                 // Crisp white text for unselected states

    public Color toggleOnGreen = new Color(0.2f, 0.85f, 0.2f, 1f);  // Vibrant active toggle green
    public Color toggleOffGray = new Color(0.25f, 0.28f, 0.35f, 1f); // Dim unlit deactivated gray

    void OnEnable()
    {
        SettingsManager.LoadSettings();
        RefreshVisualDesignLayouts();
    }

    // --- RECALCULATES THE DESIGN GRAPHICS TO MATCH SYSTEM MEMORY DATA VALUES ---
    private void RefreshVisualDesignLayouts()
    {
        // 1. Process Auto Mode Slide Toggle Design Logic State
        UpdateAutoModeUI(SettingsManager.IsAutomatic);

        // 2. Process Control Layout Selection Button State Array
        UpdateControlSelectionUI(SettingsManager.CurrentControl);
    }

    // --- EXECUTES WHEN PLAYER TAPS THE AUTO MODE BUTTON PILL FRAME TRACK ---
    public void ToggleAutoModeButtonPressed()
    {
        // Flip the current boolean register state
        bool newAutoState = !SettingsManager.IsAutomatic;
        
        SettingsManager.SaveSettings(newAutoState, SettingsManager.CurrentControl);
        UpdateAutoModeUI(newAutoState);
        UpdateCarInScene();
    }

    // --- EXECUTES WHEN PLAYER SELECTS THE LEFT BUTTON SELECTION BLOCK ---
    public void SelectButtonsLayoutPressed()
    {
        SettingsManager.SaveSettings(SettingsManager.IsAutomatic, SettingsManager.ControlType.Buttons);
        UpdateControlSelectionUI(SettingsManager.ControlType.Buttons);
        UpdateCarInScene();
    }

    // --- EXECUTES WHEN PLAYER SELECTS THE RIGHT STEERING WHEEL BLOCK ---
    public void SelectSteeringLayoutPressed()
    {
        SettingsManager.SaveSettings(SettingsManager.IsAutomatic, SettingsManager.ControlType.Steering);
        UpdateControlSelectionUI(SettingsManager.ControlType.Steering);
        UpdateCarInScene();
    }

    private void UpdateAutoModeUI(bool isAutoActive)
    {
        if (autoModeStatusIndicator != null)
        {
            autoModeStatusIndicator.color = isAutoActive ? toggleOnGreen : toggleOffGray;
            
            RectTransform indicatorRect = autoModeStatusIndicator.rectTransform;
            if (indicatorRect != null)
            {
                // --- FIXED: Use anchoredPosition instead of raw anchors to prevent shape stretching ---
                // If Auto is ON, slide handle to the right (X: 38). If OFF, slide left (X: -38)
                indicatorRect.anchoredPosition = new Vector2(isAutoActive ? 38f : -38f, 0f);
            }
        }

        if (autoModeStatusText != null)
        {
            autoModeStatusText.text = isAutoActive ? "ON" : "OFF";
            autoModeStatusText.color = isAutoActive ? toggleOnGreen : Color.gray;
        }
    }


    private void UpdateControlSelectionUI(SettingsManager.ControlType activeControl)
    {
        bool isButtonsSelected = (activeControl == SettingsManager.ControlType.Buttons);

        // Handle Left Selector Block Graphics (Buttons)
        if (buttonsLayoutOption != null)
        {
            buttonsLayoutOption.image.color = isButtonsSelected ? colorSelected : colorUnselected;
            TMP_Text t = buttonsLayoutOption.GetComponentInChildren<TMP_Text>();
            if (t != null) t.color = isButtonsSelected ? textSelectedColor : textUnselectedColor;
        }

        // Handle Right Selector Block Graphics (Steering)
        if (steeringLayoutOption != null)
        {
            steeringLayoutOption.image.color = !isButtonsSelected ? colorSelected : colorUnselected;
            TMP_Text t = steeringLayoutOption.GetComponentInChildren<TMP_Text>();
            if (t != null) t.color = !isButtonsSelected ? textSelectedColor : textUnselectedColor;
        }
    }

    private void UpdateCarInScene()
    {
        GameObject car = GameObject.FindGameObjectWithTag("Player"); 
        if (car != null)
        {
            VehicleInputHandler handler = car.GetComponent<VehicleInputHandler>();
            if (handler != null) handler.SetTransmissionMode(SettingsManager.IsAutomatic);
            
            MobileCarInput mobile = car.GetComponent<MobileCarInput>();
            if (mobile != null) mobile.UpdateControlUI();
        }
    }

    public void ClosePanel() { gameObject.SetActive(false); }
    public void OpenPanel() { gameObject.SetActive(true); }
}

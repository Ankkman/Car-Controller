using UnityEngine;  
using UnityEngine.UI; 

public class UIManager : MonoBehaviour  
{  
    private MobileCarInput mobileInput;  
    private VehicleInputHandler inputHandler; 

    [Header("Steering UI Panels (Scene References)")]  
    public GameObject buttonUICanvas;  
    public GameObject steeringUICanvas; 

    [Header("Mobile Steering Wheel Component")]  
    public MobileSteeringWheel steeringWheel; // Drag your UI steering wheel object here! 

    [Header("Dashboard Glow Indicators (Scene References)")]  
    public Image engineStatusIndicator;  
    public Image lightStatusIndicator; 

    void Start()  
    {  
        SettingsManager.LoadSettings();  
        UpdateControlLayoutUI();  
        FindCurrentCar(); 

        // Establish default dark baseline colors  
        if (engineStatusIndicator != null) engineStatusIndicator.color = new Color(0.15f, 0.15f, 0.15f, 1f);  
        if (lightStatusIndicator != null) lightStatusIndicator.color = new Color(0.15f, 0.15f, 0.15f, 1f);  
    } 

    void Update()  
    {  
        // Fallback target relinking loop in case references break during frame spikes  
        if (mobileInput == null || inputHandler == null)  
        {  
            FindCurrentCar();  
        }  
    } 

    public void SetCurrentCar(GameObject newCar)  
    {  
        if (newCar != null)  
        {  
            mobileInput = newCar.GetComponent<MobileCarInput>();  
            inputHandler = newCar.GetComponent<VehicleInputHandler>(); 

            // Pass the Canvas steering wheel over to the newly spawned car's input script  
            if (mobileInput != null && steeringWheel != null)  
            {  
                mobileInput.steeringWheel = steeringWheel;  
            } 

            // Sync UI layout matching vehicle capabilities  
            UpdateControlLayoutUI();  
            SyncIndicators();  
        }  
    } 

    public void FindCurrentCar()  
    {  
        GameObject player = GameObject.FindGameObjectWithTag("Player");  
        if (player != null)  
        {  
            mobileInput = player.GetComponent<MobileCarInput>();  
            inputHandler = player.GetComponent<VehicleInputHandler>(); 

            if (mobileInput != null && steeringWheel != null)  
            {  
                mobileInput.steeringWheel = steeringWheel;  
            } 

            SyncIndicators();  
        }  
    } 

    public void UpdateControlLayoutUI()  
    {  
        bool isSteering = SettingsManager.CurrentControl == SettingsManager.ControlType.Steering; 

        if (buttonUICanvas != null) buttonUICanvas.SetActive(!isSteering);  
        if (steeringUICanvas != null) steeringUICanvas.SetActive(isSteering);  
    } 

    private void SyncIndicators()  
    {  
        if (mobileInput != null && mobileInput.carController != null)  
        {  
            if (engineStatusIndicator != null)  
            {  
                engineStatusIndicator.color = mobileInput.carController.engineOn ? Color.green : new Color(0.15f, 0.15f, 0.15f, 1f);  
            }  
            if (lightStatusIndicator != null && mobileInput.headlightController != null)  
            {  
                bool areLightsOn = mobileInput.headlightController.leftHeadlight != null ? mobileInput.headlightController.leftHeadlight.enabled : false;  
                lightStatusIndicator.color = areLightsOn ? Color.yellow : new Color(0.15f, 0.15f, 0.15f, 1f);  
            }  
        }  
    } 

    // --- WRAPPER METHODS FOR UI BUTTONS ---  
    public void ToggleEngineMode()  
    {  
        if (mobileInput != null)  
        {  
            mobileInput.ToggleEngineMode();  
            SyncIndicators();  
        }  
        else FindCurrentCar();  
    } 

    public void ToggleHeadlightsEvent()  
    {  
        if (mobileInput != null)  
        {  
            mobileInput.ToggleHeadlightsEvent();  
            SyncIndicators();  
        }  
        else FindCurrentCar();  
    } 

    public void HornPressed() { if (mobileInput != null) mobileInput.HornPressed(); else FindCurrentCar(); }  
    public void HornReleased() { if (mobileInput != null) mobileInput.HornReleased(); else FindCurrentCar(); } 

    public void ManualShiftUp() { if (inputHandler != null) inputHandler.ManualShiftUp(); else FindCurrentCar(); }  
    public void ManualShiftDown() { if (inputHandler != null) inputHandler.ManualShiftDown(); else FindCurrentCar(); } 

    public void SteerLeftPressed() { if (mobileInput != null) mobileInput.SteerLeftPressed(); else FindCurrentCar(); }  
    public void SteerLeftReleased() { if (mobileInput != null) mobileInput.SteerLeftReleased(); else FindCurrentCar(); }  
    public void SteerRightPressed() { if (mobileInput != null) mobileInput.SteerRightPressed(); else FindCurrentCar(); }  
    public void SteerRightReleased() { if (mobileInput != null) mobileInput.SteerRightReleased(); else FindCurrentCar(); } 

    public void AcceleratePressed() { if (mobileInput != null) mobileInput.AcceleratePressed(); else FindCurrentCar(); }  
    public void AccelerateReleased() { if (mobileInput != null) mobileInput.AccelerateReleased(); else FindCurrentCar(); }  
    public void BrakePressed() { if (mobileInput != null) mobileInput.BrakePressed(); else FindCurrentCar(); }  
    public void BrakeReleased() { if (mobileInput != null) mobileInput.BrakeReleased(); else FindCurrentCar(); }  
}

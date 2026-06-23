using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCarInput : MonoBehaviour
{
    [Header("References")]
    public CarController carController;
    public Engine engine;

    [Header("Accessory Controllers")]
    public HeadlightController headlightController;
    public HornController hornController;

    // Internal variables
    private float steerValue = 0f;
    private float throttleValue = 0f;
    private float brakeValue = 0f;

    [Header("Steering Control")]
    public MobileSteeringWheel steeringWheel;
    public float steeringDeadzone = 0.05f; 

    [Header("UI Canvases to Switch")]
    public GameObject buttonUICanvas;   
    public GameObject steeringUICanvas; 

    [Header("Glow Pill Indicators")]
    public UnityEngine.UI.Image engineStatusIndicator; // Drag Engine Glow Pill here
    public UnityEngine.UI.Image lightStatusIndicator;  // Drag Headlight Glow Pill here

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (engine == null) engine = GetComponent<Engine>();

        // --- CRITICAL INITIALIZATION FIXES ---
        SettingsManager.LoadSettings(); // Load saved preferences
        UpdateControlUI();              // Turn on correct steering setup UI canvas immediately
        // -------------------------------------

        if (carController != null)
        {
            carController.useMobileInputs = true;
            carController.engineOn = false;
            carController.currentMode = CarController.TransmissionMode.Park; 
            
            brakeValue = 1f; 
        }

        // --- FIX: Force default indicators to dark gray on game startup ---
        if (engineStatusIndicator != null) 
            engineStatusIndicator.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        if (lightStatusIndicator != null) 
            lightStatusIndicator.color = new Color(0.15f, 0.15f, 0.15f, 1f);
    }

    public void UpdateControlUI()
    {
        bool isSteering = SettingsManager.CurrentControl == SettingsManager.ControlType.Steering;

        if (buttonUICanvas != null) buttonUICanvas.SetActive(!isSteering);
        if (steeringUICanvas != null) steeringUICanvas.SetActive(isSteering);
    }

    void Update()
    {
        if (carController == null) return;

        if (!carController.engineOn)
        {
            carController.mobileVerticalInput = 0f; 
            carController.mobileSteerInput = 0f;
            if (carController.brakeSystem != null) carController.brakeSystem.SetBrakeInput(1f); 
            return; 
        }

        float currentSteer = 0f;

        if (SettingsManager.CurrentControl == SettingsManager.ControlType.Steering && steeringWheel != null)
        {
            currentSteer = steeringWheel.steeringValue; 
        }
        else
        {
            currentSteer = steerValue;
        }

        float verticalInput = 0f;
        if (throttleValue > 0.01f) verticalInput = 1f; 
        else if (brakeValue > 0.01f) verticalInput = -1f; 
        else verticalInput = 0f;  

        carController.mobileVerticalInput = verticalInput;
        carController.mobileSteerInput = currentSteer; 
    }

    // --- STEERING ---
    public void SteerLeftPressed() { steerValue = -1f; }
    public void SteerLeftReleased() { if (steerValue < 0) steerValue = 0f; }

    public void SteerRightPressed() { steerValue = 1f; }
    public void SteerRightReleased() { if (steerValue > 0) steerValue = 0f; }

    // --- PEDALS ---
    public void AcceleratePressed() { throttleValue = 1f; }
    public void AccelerateReleased() { throttleValue = 0f; }

    public void BrakePressed() { brakeValue = 1f; }
    public void BrakeReleased() { brakeValue = 0f; }

    // --- ENGINE TOGGLE SYSTEM ---
    public void ToggleEngineMode()
    {
        if (carController == null) return;
        
        carController.ToggleEngineState(); 

        throttleValue = 0f;
        brakeValue = carController.engineOn ? 0f : 1f;
        steerValue = 0f;

        // Sync Engine Pill: Green when ON, Dark Gray when OFF
        if (engineStatusIndicator != null)
        {
            engineStatusIndicator.color = carController.engineOn ? Color.green : new Color(0.15f, 0.15f, 0.15f, 1f);
        }
    }

    // --- HEADLIGHT TOGGLE SYSTEM ---
    public void ToggleHeadlightsEvent()
    {
        if (headlightController != null)
        {
            headlightController.ToggleHeadlights();

            // Sync Headlight Pill: Yellow when ON, Dark Gray when OFF
            if (lightStatusIndicator != null)
            {
                bool areLightsOn = headlightController.leftHeadlight != null ? headlightController.leftHeadlight.enabled : false;
                lightStatusIndicator.color = areLightsOn ? Color.yellow : new Color(0.15f, 0.15f, 0.15f, 1f);
            }
        }
    }

    // --- ACCESSORY BUTTONS ---
    public void HornPressed() { if (hornController != null) hornController.PlayHorn(); }
    public void HornReleased() { if (hornController != null) hornController.StopHorn(); }
}

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
    public bool isParked = true; 

    [Header("UI Glow Effects")]
    public UnityEngine.UI.Outline parkOutline;
    public Color parkOffGlow = new Color(0,0,0,0);
    public Color parkOnGlow = new Color(1, 0, 0, 1);

    public UnityEngine.UI.Outline lightOutline;
    public Color lightOffGlow = new Color(0,0,0,0);
    public Color lightOnGlow = new Color(1, 1, 0, 1);

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (engine == null) engine = GetComponent<Engine>();

        if (carController != null)
        {
            carController.useMobileInputs = true;
            carController.currentMode = CarController.TransmissionMode.Park;
            carController.isParked = true;
            brakeValue = 1f; 
        }

        if (parkOutline != null) parkOutline.effectColor = parkOnGlow;
        if (lightOutline != null) lightOutline.effectColor = lightOffGlow;
    }

    void Update()
    {
        if (carController == null) return;

        // Park Mode
        if (isParked)
        {
            carController.mobileVerticalInput = 0f; 
            carController.mobileSteerInput = 0f;
            if (carController.brakeSystem != null) carController.brakeSystem.SetBrakeInput(brakeValue);
            return; 
        }

        // --- Calculate Inputs for the Car ---
        float verticalInput = 0f;

        // Gas ALWAYS overrides Brake.
        if (throttleValue > 0.01f) 
        {
            verticalInput = 1f; 
        }
        else if (brakeValue > 0.01f) 
        {
            verticalInput = -1f; 
        }
        else 
        {
            verticalInput = 0f;  
        }

        carController.mobileVerticalInput = verticalInput;
        carController.mobileSteerInput = steerValue;

        // --- CRITICAL FIX: DELETE THE LINE BELOW ---
        // if (engine != null) engine.throttleInput = throttleValue;
        // We removed it because the CarController handles the throttle/brain logic now.
        // -------------------------------------------
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

    // --- PARK MODE ---
    public void ToggleParkModeEvent() { ToggleParkMode(); }
    public void ToggleParkMode()
    {
        isParked = !isParked;
        carController.isParked = isParked;

        if (isParked)
        {
            carController.currentMode = CarController.TransmissionMode.Park;
            throttleValue = 0f;
            brakeValue = 1f;
            steerValue = 0f;
            if (parkOutline != null) parkOutline.effectColor = parkOnGlow;
        }
        else
        {
            carController.currentMode = CarController.TransmissionMode.Neutral;
            throttleValue = 0f;
            brakeValue = 0f;
            steerValue = 0f;
            if (parkOutline != null) parkOutline.effectColor = parkOffGlow;
        }
    }

    // --- HEADLIGHTS & HORN ---
    public void ToggleHeadlightsEvent()
    {
        if (headlightController != null)
        {
            headlightController.ToggleHeadlights();
            if (lightOutline != null)
            {
                bool areLightsOn = headlightController.leftHeadlight != null ? headlightController.leftHeadlight.enabled : false;
                lightOutline.effectColor = areLightsOn ? lightOnGlow : lightOffGlow;
            }
        }
    }

    public void HornPressed() { if (hornController != null) hornController.PlayHorn(); }
    public void HornReleased() { if (hornController != null) hornController.StopHorn(); }
}
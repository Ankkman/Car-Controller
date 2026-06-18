using UnityEngine;
using UnityEngine.EventSystems; // Needed for touch detection

public class MobileCarInput : MonoBehaviour
{
    [Header("References")]
    public CarController carController;
    public Engine engine;

    [Header("Accessory Controllers")]
    public HeadlightController headlightController;
    public HornController hornController;

    // Internal variables to hold input values
    private float steerValue = 0f;
    private float throttleValue = 0f;
    private float brakeValue = 0f;
    // --- FIXED: Start internal tracking state as true to match default Park mode ---
    private bool isParked = true; 

    [Header("UI Glow Effects")]
    public UnityEngine.UI.Outline parkOutline;        // Drag the Outline component here
    public Color parkOffGlow = new Color(0,0,0,0);    // Invisible black
    public Color parkOnGlow = new Color(1, 0, 0, 1);  // Bright Red

    public UnityEngine.UI.Outline lightOutline;       // Drag the Outline component here
    public Color lightOffGlow = new Color(0,0,0,0);   // Invisible black
    public Color lightOnGlow = new Color(1, 1, 0, 1); // Bright Yellow

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (engine == null) engine = GetComponent<Engine>();

        // Automatically switch the car controller to Mobile Mode
        if (carController != null)
        {
            carController.useMobileInputs = true;
            
            // Keep the car locked in Park at startup
            carController.currentMode = CarController.TransmissionMode.Park;
            carController.isParked = true;
            
            // Apply immediate initial brake pressure for mobile tracking compatibility
            brakeValue = 1f; 
        }

        // --- CRITICAL FIX: Force the UI to show the Park Glow right at startup ---
        if (parkOutline != null) 
        {
            parkOutline.effectColor = parkOnGlow;
        }

        // Ensure headlight glow is off at startup
        if (lightOutline != null)
        {
            lightOutline.effectColor = lightOffGlow;
        }
    }


    void Update()
    {
        if (carController == null) return;

        // 1. Send inputs to the CarController variables we created
        // We combine Throttle (positive) and Brake (negative) into one vertical float for the car controller logic
        float verticalInput = throttleValue - brakeValue; 
        carController.mobileVerticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
        carController.mobileSteerInput = steerValue;

        // 2. Send input to the Brake System directly
        if (carController.brakeSystem != null)
        {
            carController.brakeSystem.SetBrakeInput(brakeValue);
        }

        // 3. Send throttle to the Engine
        if (engine != null)
        {
            engine.throttleInput = throttleValue;
        }
    }

    // --- PUBLIC METHODS TO CALL FROM UI BUTTONS ---

    // Call this when pressing the "Steer Left" button
    public void SteerLeftPressed() { steerValue = -1f; }
    
    // Call this when releasing the "Steer Left" button
    public void SteerLeftReleased() 
    { 
        if (steerValue < 0) steerValue = 0f; 
    }

    // Call this when pressing the "Steer Right" button
    public void SteerRightPressed() { steerValue = 1f; }
    
    // Call this when releasing the "Steer Right" button
    public void SteerRightReleased() 
    { 
        if (steerValue > 0) steerValue = 0f; 
    }

    // Call this when pressing the "Accelerate" button
    public void AcceleratePressed() { throttleValue = 1f; }
    
    // Call this when releasing the "Accelerate" button
    public void AccelerateReleased() { throttleValue = 0f; }

    // Call this when pressing the "Brake" button
    public void BrakePressed() { brakeValue = 1f; }
    
    // Call this when releasing the "Brake" button
    public void BrakeReleased() { brakeValue = 0f; }

    // New method for EventTrigger to call
    public void ToggleParkModeEvent()
    {
        ToggleParkMode(); // This calls your existing private logic
    }

    // --- PARK MODE TOGGLE ---
    public void ToggleParkMode()
    {
        isParked = !isParked;
        carController.isParked = isParked;

        if (isParked)
        {
            carController.currentMode = CarController.TransmissionMode.Park;
            throttleValue = 0f;
            brakeValue = 1f;
            
            // Immediate override to prevent frame leaks into Reverse
            carController.mobileVerticalInput = -1f;

            if (parkOutline != null) parkOutline.effectColor = parkOnGlow;
        }
        else
        {
            carController.currentMode = CarController.TransmissionMode.Neutral;
            
            // Reset inputs to stop the car from popping into Reverse
            throttleValue = 0f;
            brakeValue = 0f;
            steerValue = 0f;
            carController.mobileVerticalInput = 0f;
            carController.mobileSteerInput = 0f;
            
            if (parkOutline != null) parkOutline.effectColor = parkOffGlow;
        }
    }

    public void ToggleHeadlightsEvent()
    {
        if (headlightController != null)
        {
            // Calls the public method to avoid private variable compiler errors
            headlightController.ToggleHeadlights();

            if (lightOutline != null)
            {
                // Safely check if the headlight component itself is currently enabled
                bool areLightsOn = headlightController.leftHeadlight != null ? headlightController.leftHeadlight.enabled : false;
                lightOutline.effectColor = areLightsOn ? lightOnGlow : lightOffGlow;
            }
        }
    }


    // --- HORN (Momentary Press & Release) ---
    // --- FIXED: Updated functions to use our looping audio setup cleanly ---
    public void HornPressed()
    {
        if (hornController != null)
        {
            hornController.PlayHorn();
        }
    }

    public void HornReleased()
    {
        if (hornController != null)
        {
            hornController.StopHorn();
        }
    }
}

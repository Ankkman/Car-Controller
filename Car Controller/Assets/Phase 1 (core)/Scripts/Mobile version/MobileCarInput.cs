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

    // Internal input trackers  
    private float steerValue = 0f;  
    private float throttleValue = 0f;  
    private float brakeValue = 0f; 

    [Header("Steering Control")]  
    public MobileSteeringWheel steeringWheel; // Linked dynamically via scene UIManager  
    public float steeringDeadzone = 0.05f; 

    void Start()  
    {  
        if (carController == null) carController = GetComponent<CarController>();  
        if (engine == null) engine = GetComponent<Engine>(); 

        SettingsManager.LoadSettings(); 

        if (carController != null)  
        {  
            carController.useMobileInputs = true;  
            carController.engineOn = false;  
            carController.currentMode = CarController.TransmissionMode.Park;  
            brakeValue = 1f;  
        }  
    } 

    public void SetCar(GameObject newCar)  
    {  
        carController = newCar.GetComponent<CarController>();  
        engine = newCar.GetComponent<Engine>();  
        headlightController = newCar.GetComponent<HeadlightController>();  
        hornController = newCar.GetComponent<HornController>();  
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

        // --- FIXED FOR LAYOUT MUTATION SYSTEM ---  
        if (SettingsManager.CurrentControl == SettingsManager.ControlType.Steering && steeringWheel != null)  
        {  
            currentSteer = steeringWheel.steeringValue;  
        }  
        else  
        {  
            // In Button Mode, explicitly read the smoothed button tracker value!  
            currentSteer = steerValue;  
        } 

        float verticalInput = 0f;  
        if (throttleValue > 0.01f) verticalInput = 1f;  
        else if (brakeValue > 0.01f) verticalInput = -1f;  
        else verticalInput = 0f; 

        carController.mobileVerticalInput = verticalInput; 

        // Push the active directional variable directly into your car controller's master tracker  
        carController.mobileSteerInput = currentSteer;  
    } 

    // --- STEERING WRAPPERS ---  
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
    } 

    // --- HEADLIGHT TOGGLE SYSTEM ---  
    public void ToggleHeadlightsEvent()  
    {  
        if (headlightController != null)  
        {  
            headlightController.ToggleHeadlights();  
        }  
    } 

    // --- ACCESSORY BUTTONS ---  
    public void HornPressed() { if (hornController != null) hornController.PlayHorn(); }  
    public void HornReleased() { if (hornController != null) hornController.StopHorn(); }  
}

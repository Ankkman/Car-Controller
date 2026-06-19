using UnityEngine;

public class VehicleInputHandler : MonoBehaviour
{
    [Header("Core References")]
    public CarController carController;
    public MobileCarInput mobileInput; 

    [Header("Transmission Mode")]
    public bool useAutomaticTransmission = true; // True = Auto, False = Manual
    
    [Header("UI References (For Manual Mode)")]
    public GameObject gearShiftButtons; // Drag your panel containing Gear Up/Down buttons here!

    private float verticalInput = 0f;

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (mobileInput == null) mobileInput = GetComponent<MobileCarInput>();

        // Load the saved setting on game start
        SettingsManager.LoadSettings();
        useAutomaticTransmission = SettingsManager.IsAutomatic;
        carController.isAutomaticMode = useAutomaticTransmission;

        // Hide/Show Gear buttons based on loaded mode
        if(gearShiftButtons != null)
            gearShiftButtons.SetActive(!useAutomaticTransmission); 

        if (mobileInput != null && carController != null)
        {
            mobileInput.isParked = carController.isParked;
        }
    }

    // --- NEW: PUBLIC METHOD TO SWITCH MODES INSTANTLY ---
    public void SetTransmissionMode(bool isAuto)
    {
        useAutomaticTransmission = isAuto;
        carController.isAutomaticMode = isAuto;

        // Immediately hide the Gear buttons if Auto is turned on
        if(gearShiftButtons != null)
            gearShiftButtons.SetActive(!isAuto);
    }

    void Update()
    {
        if (carController == null) return;
        if (carController.isParked) return; 

        // Get input from Mobile or PC
        if (mobileInput != null && carController.useMobileInputs) {
            verticalInput = carController.mobileVerticalInput; 
        } else {
            verticalInput = Input.GetAxis("Vertical");
            carController.mobileSteerInput = Input.GetAxis("Horizontal");
        }

        float speed = Mathf.Abs(carController.ForwardSpeed);

        if (useAutomaticTransmission)
        {
            if (verticalInput > 0.1f) 
            {
                if (carController.currentMode == CarController.TransmissionMode.Reverse && speed > 0.2f)
                {
                    // Stay in Reverse to let brakes stop the car
                }
                else
                {
                    carController.currentMode = CarController.TransmissionMode.Drive;
                }
            }
            else if (verticalInput < -0.1f) 
            {
                if (speed < carController.transmissionSwitchSpeed)
                {
                    carController.currentMode = CarController.TransmissionMode.Reverse;
                }
            }
            else if (speed < 0.2f && carController.currentMode != CarController.TransmissionMode.Park)
            {
                carController.currentMode = CarController.TransmissionMode.Neutral;
            }
        }
    }

    // --- MANUAL MODE BUTTONS ---
    public void ManualShiftUp()
    {
        if (carController == null || carController.isParked || useAutomaticTransmission) return;

        if (carController.currentMode == CarController.TransmissionMode.Reverse)
            carController.currentMode = CarController.TransmissionMode.Neutral;
        else if (carController.currentMode == CarController.TransmissionMode.Neutral)
            carController.currentMode = CarController.TransmissionMode.Drive;
    }

    public void ManualShiftDown()
    {
        if (carController == null || carController.isParked || useAutomaticTransmission) return;

        if (carController.currentMode == CarController.TransmissionMode.Drive)
            carController.currentMode = CarController.TransmissionMode.Neutral;
        else if (carController.currentMode == CarController.TransmissionMode.Neutral)
            carController.currentMode = CarController.TransmissionMode.Reverse;
    }
}
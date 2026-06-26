using UnityEngine;

public class VehicleInputHandler : MonoBehaviour
{
    [Header("Core References")]
    public CarController carController;
    public MobileCarInput mobileInput; 
    public Engine engine; 

    [Header("Transmission Mode")]
    public bool useAutomaticTransmission = true; // True = Auto, False = Manual
    
    [Header("UI References (For Manual Mode)")]
    public GameObject gearShiftButtons; 

    [Header("PC Controls")]
    public KeyCode shiftUpKey = KeyCode.E;
    public KeyCode shiftDownKey = KeyCode.Q;

    private float verticalInput = 0f;
    private int manualGear = 0; // -1 = Reverse, 0 = Neutral, 1-6 = 1st to 6th Gear

    public int CurrentManualGear => manualGear;

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (mobileInput == null) mobileInput = GetComponent<MobileCarInput>();
        if (engine == null) engine = GetComponent<Engine>();

        SettingsManager.LoadSettings();
        useAutomaticTransmission = SettingsManager.IsAutomatic;
        
        // Sync everything at start based on saved settings
        SyncTransmissionSettings(useAutomaticTransmission);

        if(gearShiftButtons != null)
            gearShiftButtons.SetActive(!useAutomaticTransmission); 
    }

    public void SetTransmissionMode(bool isAuto)
    {
        useAutomaticTransmission = isAuto;
        SyncTransmissionSettings(isAuto);
        
        if(gearShiftButtons != null)
            gearShiftButtons.SetActive(!isAuto);
    }

    private void SyncTransmissionSettings(bool isAuto)
    {
        if (carController != null)
        {
            carController.isAutomaticMode = isAuto;
        }
        if (engine != null)
        {
            engine.automatic = isAuto; 
        }
    }

    void Update()
    {
        if (carController == null) return;
        if (!carController.engineOn) return; 

        // Handle PC keyboard shifting inputs while in Manual Mode
        if (!useAutomaticTransmission)
        {
            if (Input.GetKeyDown(shiftUpKey))
            {
                ManualShiftUp();
            }
            if (Input.GetKeyDown(shiftDownKey))
            {
                ManualShiftDown();
            }
        }

        if (mobileInput != null && carController.useMobileInputs) {
            verticalInput = carController.mobileVerticalInput; 
        } else {
            verticalInput = Input.GetAxis("Vertical");
            carController.mobileSteerInput = Input.GetAxis("Horizontal");
        }

        float speed = Mathf.Abs(carController.ForwardSpeed);

        // --- AUTO MODE ---
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
        // --- MANUAL MODE ---
        else
        {
            // Note: Safety Auto-Downshift removed to mimic authentic Forza-style gameplay.
            
            // If we are in a valid drive gear, tell the Engine
            if (carController.currentMode == CarController.TransmissionMode.Drive && manualGear >= 1)
            {
                if (engine != null)
                    engine.SetManualGear(manualGear - 1); // Convert 1-6 to 0-5 array index
            }
        }
    }

    // --- MANUAL MODE BUTTONS & KEYS ---

    public void ManualShiftUp()
    {
        if (carController == null || !carController.engineOn || useAutomaticTransmission || engine == null) return;

        // R -> N
        if (manualGear == -1)
        {
            manualGear = 0;
            carController.currentMode = CarController.TransmissionMode.Neutral;
            engine.SetManualGear(-1);
        }
        // N -> 1st
        else if (manualGear == 0)
        {
            manualGear = 1;
            carController.currentMode = CarController.TransmissionMode.Drive;
            engine.SetManualGear(0);
        }
        // 1st -> 2nd ... 5th -> 6th
        else if (manualGear >= 1 && manualGear < 6)
        {
            manualGear++;
            engine.SetManualGear(manualGear - 1);
        }
    }

    public void ManualShiftDown()
    {
        if (carController == null || !carController.engineOn || useAutomaticTransmission || engine == null) return;

        // 6th -> 5th ... 2nd -> 1st
        if (manualGear > 1)
        {
            // --- FORZA DOWNSHIFT PROTECTION ---
            float nextGearRatio = engine.gears[manualGear - 2].ratio;
            float currentWheelRPM = Mathf.Abs(carController.ForwardSpeed * 60f / (2f * Mathf.PI * 0.33f)); 
            float prospectiveRPM = currentWheelRPM * nextGearRatio * engine.finalDriveRatio;

            // If the drop will redline the engine past a safe limit (7500 RPM), block it!
            if (prospectiveRPM > 7500f)
            {
                Debug.LogWarning($"Downshift Denied! Estimated RPM ({prospectiveRPM:F0}) exceeds absolute safe limit (7500).");
                
                // NEW: Find the audio controller and trigger the procedural over-rev scream!
                EngineAudioController audioController = GetComponent<EngineAudioController>();
                if (audioController != null)
                {
                    audioController.TriggerEngineMisShiftScream();
                }

                return; // Blocks the shift
            }

            // ----------------------------------

            manualGear--;
            engine.SetManualGear(manualGear - 1);
        }
        // 1st -> N
        else if (manualGear == 1)
        {
            manualGear = 0;
            carController.currentMode = CarController.TransmissionMode.Neutral;
            engine.SetManualGear(-1);
        }
        // N -> R
        else if (manualGear == 0)
        {
            manualGear = -1;
            carController.currentMode = CarController.TransmissionMode.Reverse;
            engine.SetManualGear(-1);
        }
    }

}

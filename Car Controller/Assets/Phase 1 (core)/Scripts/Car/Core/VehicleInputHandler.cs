using UnityEngine; 

public class VehicleInputHandler : MonoBehaviour  
{  
    [Header("Core References")]  
    public CarController carController;  
    public MobileCarInput mobileInput;  
    public Engine engine; 

    [Header("Transmission Mode")]  
    public bool useAutomaticTransmission = true; 

    [Header("UI References (Automatically Linked)")]  
    private GameObject gearShiftButtons; 

    [Header("PC Controls")]  
    public KeyCode shiftUpKey = KeyCode.E;  
    public KeyCode shiftDownKey = KeyCode.Q; 

    private float verticalInput = 0f;  
    private int manualGear = 0; 

    public int CurrentManualGear => manualGear; 

    void Start()  
    {  
        if (carController == null) carController = GetComponent<CarController>();  
        if (mobileInput == null) mobileInput = GetComponent<MobileCarInput>();  
        if (engine == null) engine = GetComponent<Engine>(); 

        // DYNAMIC UI LINKING: Find the canvas UI element in the scene deep nested  
        GameObject uiManager = GameObject.FindGameObjectWithTag("InGameUI");  
        if (uiManager != null)  
        {  
            // --- FIXED: Use recursive search to find GearShiftPanel deep inside SteeringGroup ---  
            Transform panelTransform = FindChildWithName(uiManager.transform, "GearShiftPanel");  
            if (panelTransform != null) gearShiftButtons = panelTransform.gameObject;  
        } 

        SettingsManager.LoadSettings();  
        useAutomaticTransmission = SettingsManager.IsAutomatic; 

        SyncTransmissionSettings(useAutomaticTransmission); 

        if (gearShiftButtons != null)  
        {
            gearShiftButtons.SetActive(!useAutomaticTransmission);  
        }
    } 

    private Transform FindChildWithName(Transform parent, string targetName)  
    {  
        if (parent.name == targetName) return parent; 

        foreach (Transform child in parent)  
        {  
            Transform result = FindChildWithName(child, targetName);  
            if (result != null) return result;  
        }  
        return null;  
    } 

    public void SetTransmissionMode(bool isAuto)  
    {  
        useAutomaticTransmission = isAuto;  
        SyncTransmissionSettings(isAuto); 

        if (gearShiftButtons != null)  
        {
            gearShiftButtons.SetActive(!isAuto);  
        }
    } 

    private void SyncTransmissionSettings(bool isAuto)  
    {  
        if (carController != null) carController.isAutomaticMode = isAuto;  
        if (engine != null) engine.automatic = isAuto;  
    } 

    void Update()  
    {  
        if (carController == null || !carController.engineOn) return; 

        if (!useAutomaticTransmission)  
        {  
            if (Input.GetKeyDown(shiftUpKey)) ManualShiftUp();  
            if (Input.GetKeyDown(shiftDownKey)) ManualShiftDown();  
        } 

        if (mobileInput != null && carController.useMobileInputs) 
        {  
            verticalInput = carController.mobileVerticalInput;  
        } 
        else 
        {  
            verticalInput = Input.GetAxis("Vertical");  
            carController.mobileSteerInput = Input.GetAxis("Horizontal");  
        } 

        float speed = Mathf.Abs(carController.ForwardSpeed); 

        if (useAutomaticTransmission)  
        {  
            if (verticalInput > 0.1f)  
            {  
                if (carController.currentMode != CarController.TransmissionMode.Reverse || speed <= 0.2f)  
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
        else  
        {  
            if (carController.currentMode == CarController.TransmissionMode.Drive && manualGear >= 1)  
            {  
                if (engine != null) engine.SetManualGear(manualGear - 1);  
            }  
        }  
    } 

    public void ManualShiftUp()  
    {  
        if (carController == null || !carController.engineOn || useAutomaticTransmission || engine == null) return; 

        if (manualGear == -1)  
        {  
            manualGear = 0;  
            carController.currentMode = CarController.TransmissionMode.Neutral;  
            engine.SetManualGear(-1);  
        }  
        else if (manualGear == 0)  
        {  
            manualGear = 1;  
            carController.currentMode = CarController.TransmissionMode.Drive;  
            engine.SetManualGear(0);  
        }  
        else if (manualGear >= 1 && manualGear < 6)  
        {  
            manualGear++;  
            engine.SetManualGear(manualGear - 1);  
        }  
    } 

    public void ManualShiftDown()  
    {  
        if (carController == null || !carController.engineOn || useAutomaticTransmission || engine == null) return; 

        if (manualGear > 1)  
        {  
            float nextGearRatio = engine.gears[manualGear - 2].ratio;  
            float currentWheelRPM = Mathf.Abs(carController.ForwardSpeed * 60f / (2f * Mathf.PI * 0.33f));  
            float prospectiveRPM = currentWheelRPM * nextGearRatio * engine.finalDriveRatio; 

            if (prospectiveRPM > 7500f)  
            {  
                Debug.LogWarning("Downshift Denied! Over-rev protection.");  
                EngineAudioController audioController = GetComponent<EngineAudioController>();  
                if (audioController != null) audioController.TriggerEngineMisShiftScream();  
                return;  
            } 

            manualGear--;  
            engine.SetManualGear(manualGear - 1);  
        }  
        else if (manualGear == 1)  
        {  
            manualGear = 0;  
            carController.currentMode = CarController.TransmissionMode.Neutral;  
            engine.SetManualGear(-1);  
        }  
        else if (manualGear == 0)  
        {  
            manualGear = -1;  
            carController.currentMode = CarController.TransmissionMode.Reverse;  
            engine.SetManualGear(-1);  
        }  
    }  
}

using UnityEngine; 

public class CameraModeController : MonoBehaviour  
{  
    [Header("Camera Modes")]  
    public SimpleFollow thirdPersonCameraScript; // Drag your camera's SimpleFollow script component here 

    [Header("Driver View Settings")]  
    public float steeringLookAmount = 12f;  
    public float steeringSmooth = 5f;  
    public float driverFOV = 75f;  
    public float thirdPersonFOV = 60f; 

    // Runtime Dynamic Target References  
    private Transform activeDriverViewPoint;  
    private Transform activeInteriorSteeringWheel;  
    private CarController activeCarController; 

    private Camera cam;  
    private bool isDriverModeActive = false;  
    private float currentLookAngle; 

    // Caches the original local rotation of the spawned steering wheel  
    private Quaternion originalWheelLocalRotation; 

    void Start()  
    {  
        cam = GetComponent<Camera>(); 

        if (thirdPersonCameraScript == null)  
        {  
            thirdPersonCameraScript = GetComponent<SimpleFollow>();  
        } 

        FindAndLinkActiveCar();  
    } 

    void Update()  
    {  
        // SYSTEM RUNTIME LINK: If our car target goes missing or swapped, find the new one instantly  
        if (activeCarController == null || activeDriverViewPoint == null)  
        {  
            FindAndLinkActiveCar();  
        } 

        // Camera mode toggle trigger (Works via PC keyboard "C" or public method call for UI buttons)  
        if (Input.GetKeyDown(KeyCode.C))  
        {  
            ToggleCameraViewMode();  
        } 

        if (isDriverModeActive)  
        {  
            UpdateDriverCameraPosition();  
            UpdateInteriorSteeringWheelRotation();  
        }  
    } 

    public void ToggleCameraViewMode()  
    {  
        isDriverModeActive = !isDriverModeActive; 

        if (isDriverModeActive)  
        {  
            if (thirdPersonCameraScript != null) thirdPersonCameraScript.enabled = false;  
            if (cam != null) cam.fieldOfView = driverFOV;  
        }  
        else  
        {  
            if (thirdPersonCameraScript != null) thirdPersonCameraScript.enabled = true;  
            if (cam != null) cam.fieldOfView = thirdPersonFOV;  
        }  
    } 

    private void FindAndLinkActiveCar()  
    {  
        GameObject playerVehicle = GameObject.FindGameObjectWithTag("Player");  
        if (playerVehicle != null)  
        {  
            activeCarController = playerVehicle.GetComponent<CarController>(); 

            activeDriverViewPoint = FindChildWithNameRecursive(playerVehicle.transform, "DriverViewPoint"); 

            Transform newWheel = FindChildWithNameRecursive(playerVehicle.transform, "InteriorSteeringWheel");  
            if (newWheel != activeInteriorSteeringWheel)  
            {  
                activeInteriorSteeringWheel = newWheel; 

                if (activeInteriorSteeringWheel != null)  
                {  
                    originalWheelLocalRotation = activeInteriorSteeringWheel.localRotation;  
                }  
            }  
        }  
    } 

    private Transform FindChildWithNameRecursive(Transform parent, string targetName)  
    {  
        if (parent.name == targetName) return parent; 

        foreach (Transform child in parent)  
        {  
            Transform result = FindChildWithNameRecursive(child, targetName);  
            if (result != null) return result;  
        }  
        return null;  
    } 

    private void UpdateDriverCameraPosition()  
    {  
        if (activeDriverViewPoint == null) return; 

        transform.position = activeDriverViewPoint.position; 

        // --- FIXED FOR ALL LAYOUTS ---  
        // Reads the absolute true computed steering value running tire mechanics  
        float steeringInput = GetTrueSteeringValue(); 

        float targetAngle = steeringInput * steeringLookAmount;  
        currentLookAngle = Mathf.Lerp(currentLookAngle, targetAngle, Time.deltaTime * steeringSmooth); 

        transform.rotation = activeDriverViewPoint.rotation * Quaternion.Euler(0, currentLookAngle, 0);  
    } 

    private void UpdateInteriorSteeringWheelRotation()  
    {  
        if (activeInteriorSteeringWheel == null || activeCarController == null) return; 

        // --- FIXED FOR ALL LAYOUTS ---  
        // Pulls the absolute final steering data right from the engine matrix pipeline  
        float currentSteeringValue = GetTrueSteeringValue(); 

        float visualWheelRotationAngle = currentSteeringValue * 360f; 

        activeInteriorSteeringWheel.localRotation = originalWheelLocalRotation * Quaternion.Euler(0f, 0f, visualWheelRotationAngle);  
    } 

    private float GetTrueSteeringValue()  
    {  
        if (activeCarController == null) return 0f; 

        // --- FIXED: Directly requests the true, actual steering angle calculated by the car ---  
        return activeCarController.CurrentSteerInput;  
    }  
}

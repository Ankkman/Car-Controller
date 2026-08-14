using UnityEngine; 

public class CameraModeController : MonoBehaviour  
{  
    [Header("Camera Modes")]  
    public FreeLookCamera thirdPersonCameraScript; // Now tracking our FreeLookCamera component!

    [Header("Driver View Settings")]  
    public float steeringLookAmount = 12f;  
    public float steeringSmooth = 5f;  
    public float driverFOV = 75f;  
    public float thirdPersonFOV = 60f; 

    private Transform activeDriverViewPoint;  
    private Transform activeInteriorSteeringWheel;  
    private CarController activeCarController; 

    private Camera cam;  
    private bool isDriverModeActive = false;  
    private float currentLookAngle; 

    private Quaternion originalWheelLocalRotation; 

    void Start()  
    {  
        cam = GetComponent<Camera>(); 

        if (thirdPersonCameraScript == null)  
        {  
            thirdPersonCameraScript = GetComponent<FreeLookCamera>();  
        } 

        FindAndLinkActiveCar();  
    } 

    void Update()  
    {  
        if (activeCarController == null || activeDriverViewPoint == null)  
        {  
            FindAndLinkActiveCar();  
        } 

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
            // --- FIX: Stop FreeLookCamera calculations completely so swipe rotation is blocked ---
            if (thirdPersonCameraScript != null) thirdPersonCameraScript.isControlDisabled = true;  
            if (cam != null) cam.fieldOfView = driverFOV;  
        }  
        else  
        {  
            // --- FIX: Hand control back over to Third Person FreeLook tracking ---
            if (thirdPersonCameraScript != null) thirdPersonCameraScript.isControlDisabled = false;  
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

        float steeringInput = GetTrueSteeringValue(); 
        float targetAngle = steeringInput * steeringLookAmount;  
        currentLookAngle = Mathf.Lerp(currentLookAngle, targetAngle, Time.deltaTime * steeringSmooth); 

        transform.rotation = activeDriverViewPoint.rotation * Quaternion.Euler(0, currentLookAngle, 0);  
    } 

    private void UpdateInteriorSteeringWheelRotation()  
    {  
        if (activeInteriorSteeringWheel == null || activeCarController == null) return; 

        float currentSteeringValue = GetTrueSteeringValue(); 
        float visualWheelRotationAngle = currentSteeringValue * 360f; 

        activeInteriorSteeringWheel.localRotation = originalWheelLocalRotation * Quaternion.Euler(0f, 0f, visualWheelRotationAngle);  
    } 

    private float GetTrueSteeringValue()  
    {  
        if (activeCarController == null) return 0f; 
        return activeCarController.CurrentSteerInput;  
    }  
}

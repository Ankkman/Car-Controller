using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Target & Setup")]
    public Transform target;
    public Vector3 cameraOffset = new Vector3(0, 3.5f, -6f);

    [Header("Realistic Lag Settings")]
    [Tooltip("Lower values mean tighter follow. Higher values mean more realistic speed lag.")]
    public float positionSmoothTime = 0.08f; 
    private Vector3 currentPivotVelocity;

    [Header("Swipe Settings")]
    public float lookSensitivity = 5f;
    public float smoothReturnSpeed = 6f;

    [Header("State Settings")]
    public bool isControlDisabled = false;

    private float currentYaw = 0f;
    private float targetYaw = 0f;
    private bool isDragging = false;
    private Vector2 lastTouchPos;
    private Transform cameraPivot;

    [Header("Smoothness & Cinematic Feel")]
    [Tooltip("How smoothly the camera follows the car's Y-axis height (suspension feel).")]
    public float verticalSmoothSpeed = 6f;
    [Tooltip("How smoothly the camera rotates to look at the car.")]
    public float rotationFollowSpeed = 8f;

    // Internal tracking variables
    private float currentVerticalPos;
    private Quaternion targetLookRotation;

    void Start()
    {
        GameObject pivotObj = new GameObject("CameraPivot_Runtime");
        cameraPivot = pivotObj.transform;
        FindActiveTarget();
        
        // Snap immediately on start to prevent a huge jump
        if (target != null)
        {
            cameraPivot.position = target.position;
        }
    }

    public void SetCar(GameObject newCar)
    {
        FindActiveTarget(newCar);
    }

    private void FindActiveTarget(GameObject specificCar = null)
    {
        GameObject spawnedPlayer = specificCar != null ? specificCar : GameObject.FindGameObjectWithTag("Player");
        if (spawnedPlayer != null)
        {
            Transform customAnchor = spawnedPlayer.transform.Find("CameraLookTarget");
            if (customAnchor != null)
                target = customAnchor;
            else
                target = spawnedPlayer.transform;
        }
    }

    void LateUpdate()
    {
        if (isControlDisabled || target == null) return;

        // --- HYBRID POSITION TRACKING ---
        // X and Z are instant (prevents jitter).
        // Y is smoothed (gives a nice suspension float feel).
        currentVerticalPos = Mathf.Lerp(currentVerticalPos, target.position.y, Time.deltaTime * verticalSmoothSpeed);
        cameraPivot.position = new Vector3(target.position.x, currentVerticalPos, target.position.z);

        // --- Input cleanup ---
        if (isDragging && Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            isDragging = false;
        }

        // --- Lerp yaw generation ---
        if (isDragging)
        {
            currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * lookSensitivity * 2f);
        }
        else
        {
            currentYaw = Mathf.Lerp(currentYaw, 0f, Time.deltaTime * smoothReturnSpeed);
            targetYaw = currentYaw;
        }

        // --- Rotate the pivot ---
        Quaternion carHeadingRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        cameraPivot.rotation = carHeadingRotation * Quaternion.Euler(0f, currentYaw, 0f);

        // --- Calculate camera position ---
        Vector3 finalTargetCameraPos = cameraPivot.position + (cameraPivot.rotation * cameraOffset);
        transform.position = finalTargetCameraPos;

        // --- CINEMATIC ROTATION LAG ---
        // Instead of snapping instantly, the camera smoothly rotates to look at the car.
        targetLookRotation = Quaternion.LookRotation(cameraPivot.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, Time.deltaTime * rotationFollowSpeed);
    }

    public void StartDragging(Vector2 screenPosition)
    {
        if (isControlDisabled) return;
        isDragging = true;
        lastTouchPos = screenPosition;
    }

    public void UpdateDragging(Vector2 screenPosition)
    {
        if (!isDragging || isControlDisabled) return;
        float deltaX = screenPosition.x - lastTouchPos.x;
        targetYaw += deltaX * lookSensitivity * 0.01f;
        lastTouchPos = screenPosition;
    }
}

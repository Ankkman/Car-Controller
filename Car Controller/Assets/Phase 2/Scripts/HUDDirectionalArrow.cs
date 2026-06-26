using UnityEngine;

public class HUDDirectionalArrow : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("Drag your main Player Vehicle object here.")]
    public Transform playerVehicle;
    
    [Tooltip("Drag your _MissionManager object here.")]
    public MissionManager missionManager;

    [Header("Positioning Adjustments")]
    [Tooltip("How high above the car should the arrow float?")]
    public float heightOffset = 2.5f;
    
    [Tooltip("Push the arrow forward or backward relative to the car (- values move it backward closer to camera view).")]
    public float forwardOffset = -0.5f;

    [Header("Rotation Tuning")]
    [Tooltip("How fast should the arrow rotate toward the target?")]
    public float rotationSettleSpeed = 8f;

    [Tooltip("CRUCIAL: Add a permanent manual rotation tilt (X = Tilt forward/down, Y = Spin, Z = Roll) to make it face the camera perfectly!")]
    public Vector3 manualRotationOffset = new Vector3(20f, 0f, 0f);

    [Header("Idle Animation Settings")]
    [Tooltip("How fast should the arrow bounce up and down in mid-air?")]
    public float hoverSpeed = 3f;
    [Tooltip("The maximum distance the arrow will float up and down.")]
    public float hoverAmplitude = 0.15f;

    void LateUpdate()
    {
        // Fallback protection loops
        if (playerVehicle == null || missionManager == null) return;

        // 1. POSITIONING LOGIC WITH FORWARD/BACKWARD BIAS
        // Calculates a clean position overhead that shifts along with the car's orientation matrix
        Vector3 targetPosition = playerVehicle.position + (Vector3.up * heightOffset) + (playerVehicle.forward * forwardOffset);
        
        // Add a smooth hovering bounce effect
        float bobbingEffect = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        targetPosition.y += bobbingEffect;
        
        transform.position = targetPosition;

        // 2. TARGET LOOK-AT ROTATION WITH CUSTOM TILT HOOKS
        Transform targetWaypoint = missionManager.ActiveTargetTransform;

        if (targetWaypoint != null)
        {
            // Flatten the direction vector on the Y-axis so tracking remains horizontal
            Vector3 directionToTarget = targetWaypoint.position - transform.position;
            directionToTarget.y = 0; 

            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                // Core baseline tracking rotation targeting our coordinates
                Quaternion baseTargetRotation = Quaternion.LookRotation(directionToTarget);
                
                // Inject your custom designer tilt offset angles on top of the calculated target heading
                Quaternion finalTargetRotation = baseTargetRotation * Quaternion.Euler(manualRotationOffset);
                
                // Blend smoothly to prevent sudden visual snapping
                transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotationSettleSpeed);
            }
        }
        else
        {
            // If all missions are completed or inactive, spin the arrow slowly in neutral circles
            transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}

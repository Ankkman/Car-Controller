using UnityEngine;

public class AdvancedCameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;
    private Rigidbody targetRigidbody;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 3f, -7f);
    public float positionSmoothTime = 0.3f; // Time to reach target (lower = faster)
    
    [Header("Rotation Settings")]
    public float rotationSmoothSpeed = 5f;

    [Header("Speed FX (FOV)")]
    public Camera cam;
    public float baseFOV = 60f;
    public float maxFOVOffset = 15f;
    public float fovMaxSpeed = 40f; // Speed at which FOV is maxed out

    [Header("Camera Shake FX")]
    public float shakeIntensity = 0.05f;
    public float shakeMinSpeed = 20f; // Speed where shaking begins

    private Vector3 currentVelocity;

    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (target != null) targetRigidbody = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate vehicle speed in meters per second or MPH/KPH magnitude
        float currentSpeed = targetRigidbody != null ? targetRigidbody.linearVelocity.magnitude : 0f;

        // 2. Smooth Position using SmoothDamp (better handling than Lerp for camera trailing)
        Vector3 targetPosition = target.position + (target.rotation * offset);
        
        // Apply perlin noise shake based on speed
        if (currentSpeed > shakeMinSpeed)
        {
            float speedFactor = Mathf.InverseLerp(shakeMinSpeed, fovMaxSpeed, currentSpeed);
            float shakeX = (Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f) * shakeIntensity * speedFactor;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * 20f) - 0.5f) * shakeIntensity * speedFactor;
            targetPosition += new Vector3(shakeX, shakeY, 0f);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);

        // 3. Smooth LookAt Rotation (prevents snapping)
        Vector3 direction = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);

        // 4. Speed Stretch FOV Effect
        if (cam != null)
        {
            float targetFOV = baseFOV + (Mathf.Clamp01(currentSpeed / fovMaxSpeed) * maxFOVOffset);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 2f);
        }
    }
}

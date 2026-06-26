using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform carTarget;          // The car to follow
    public Vector3 offset = new Vector3(0, 80, 0); // Height above car
    public bool followCar = true;

    [Header("Zoom")]
    public float minZoom = 20f;          // Most zoomed in (shows less area)
    public float maxZoom = 100f;         // Most zoomed out (shows more area)
    public float currentZoom = 60f;
    public float zoomSpeed = 20f;
    public float zoomInputSensitivity = 1f;

    [Header("Rotation")]
    public bool rotateWithCar = false;   // True = minimap rotates with car direction
    public float rotationSmoothSpeed = 5f;

    [Header("Smoothing")]
    public bool smoothFollow = true;
    public float followSpeed = 8f;

    private Camera minimapCamera;
    private float targetZoom;

    void Start()
    {
        minimapCamera = GetComponent<Camera>();
        if (minimapCamera == null)
        {
            Debug.LogError("MinimapCameraController requires a Camera component!");
            enabled = false;
            return;
        }

        // Ensure orthographic mode
        minimapCamera.orthographic = true;
        targetZoom = currentZoom;
        minimapCamera.orthographicSize = currentZoom;
    }

    void LateUpdate()
    {
        if (carTarget == null)
        {
            Debug.LogWarning("MinimapCameraController has no target assigned!");
            return;
        }

        HandleZoom();
        FollowTarget();
        HandleRotation();
    }

    void HandleZoom()
    {
        // Mouse scroll wheel for zooming
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetZoom -= scrollInput * zoomSpeed * zoomInputSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smooth zoom transition
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * 5f);
        minimapCamera.orthographicSize = currentZoom;
    }

    void FollowTarget()
    {
        Vector3 targetPosition = carTarget.position + offset;

        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    void HandleRotation()
    {
        if (rotateWithCar)
        {
            // Get car's forward direction (Y-axis rotation)
            float targetYRotation = carTarget.eulerAngles.y;

            if (smoothFollow)
            {
                Quaternion targetRotation = Quaternion.Euler(90f, targetYRotation, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Euler(90f, targetYRotation, 0f);
            }
        }
        else
        {
            // North-locked (static rotation)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    // Public methods for external control
    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void SetTarget(Transform newTarget)
    {
        carTarget = newTarget;
    }

    public void ToggleRotation()
    {
        rotateWithCar = !rotateWithCar;
    }
}
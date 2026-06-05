using UnityEngine;

public class CameraModeController : MonoBehaviour
{
    public SimpleFollow thirdPersonCamera;

    public Transform driverViewPoint;
    public Transform driverCameraPivot;

    public CarController carController;

    private Camera cam;

    private bool driverMode = false;

    [Header("Driver Camera")]
    public float steeringLookAmount = 12f;
    public float steeringSmooth = 5f;

    private float currentLookAngle;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            driverMode = !driverMode;

            if (driverMode)
            {
                thirdPersonCamera.enabled = false;
                cam.fieldOfView = 75f;
            }
            else
            {
                thirdPersonCamera.enabled = true;
                cam.fieldOfView = 60f;
            }
        }

        if (driverMode)
        {
            UpdateDriverCamera();
        }
    }

    void UpdateDriverCamera()
    {
        cam.transform.position =
            driverViewPoint.position;

        float steer =
            Input.GetAxis("Horizontal");

        float targetAngle =
            steer * steeringLookAmount;

        currentLookAngle =
            Mathf.Lerp(
                currentLookAngle,
                targetAngle,
                Time.deltaTime * steeringSmooth
            );

        cam.transform.rotation =
            driverViewPoint.rotation *
            Quaternion.Euler(
                0,
                currentLookAngle,
                0
            );
    }
}
using UnityEngine;

public class SpeedDebugger : MonoBehaviour
{
    private Rigidbody rb;
    
    [Header("Display Settings")]
    public bool showDebugGUI = true;
    public bool showConsoleLog = false;

    [Header("Wheel Radius Check")]
    public WheelCollider referenceWheel;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Print wheel radius for verification
        if (referenceWheel != null)
        {
            Debug.Log($"Wheel Radius: {referenceWheel.radius} meters");
        }
    }

    void Update()
    {
        if (showConsoleLog)
        {
            float speedMS = rb.linearVelocity.magnitude;
            float speedKMH = speedMS * 3.6f;
            float speedMPH = speedMS * 2.237f;
            Debug.Log($"Speed: {speedMS:F1} m/s | {speedKMH:F1} km/h | {speedMPH:F1} mph | RPM: {GetComponent<Engine>()?.EngineRPM ?? 0}");
        }
    }

    void OnGUI()
    {
        if (!showDebugGUI) return;

        float speedMS = rb.linearVelocity.magnitude;
        float speedKMH = speedMS * 3.6f;
        float speedMPH = speedMS * 2.237f;

        GUI.Box(new Rect(10, 10, 300, 150), "SPEED DIAGNOSTICS");
        GUI.Label(new Rect(20, 30, 280, 20), $"Rigidbody Velocity: {speedMS:F2} m/s");
        GUI.Label(new Rect(20, 50, 280, 20), $"Speed: {speedKMH:F0} km/h");
        GUI.Label(new Rect(20, 70, 280, 20), $"Speed: {speedMPH:F0} mph");
        GUI.Label(new Rect(20, 90, 280, 20), $"Wheel Radius: {(referenceWheel ? referenceWheel.radius : 0):F3} m");
        GUI.Label(new Rect(20, 110, 280, 20), $"Car Mass: {rb.mass} kg");
        
        if (referenceWheel != null && referenceWheel.isGrounded)
        {
            WheelHit hit;
            referenceWheel.GetGroundHit(out hit);
            GUI.Label(new Rect(20, 130, 280, 20), $"Forward Slip: {hit.forwardSlip:F3}");
        }
    }
}
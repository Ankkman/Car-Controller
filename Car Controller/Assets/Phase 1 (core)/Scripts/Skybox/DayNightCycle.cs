using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun Settings")]
    public Light directionalSun;
    public float dayLengthInSeconds = 120f; // A full day/night loop takes 2 minutes

    [Header("Light Adjustments")]
    public float maxIntensity = 1.3f;
    public float minIntensity = 0.05f;

    private float rotationSpeed;

    void Start()
    {
        // Calculate degrees per second needed to rotate 360 degrees
        rotationSpeed = 360f / dayLengthInSeconds;

        // If not assigned manually, try to automatically find the main Sun light
        if (directionalSun == null)
        {
            directionalSun = GetComponent<Light>();
        }
    }

    void Update()
    {
        if (directionalSun == null) return;

        // 1. Rotate the sun light along its local X-axis to simulate movement across the sky
        directionalSun.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        // 2. Adjust light brightness dynamically based on angle (Dim at night, bright at noon)
        float currentAngleX = directionalSun.transform.localEulerAngles.x;

        // Check if the sun has dipped below the artificial horizon line
        if (currentAngleX > 180f)
        {
            // Nighttime: Dim the sun light completely
            directionalSun.intensity = Mathf.MoveTowards(directionalSun.intensity, minIntensity, Time.deltaTime);
        }
        else
        {
            // Daytime: Fade light back up to maximum warmth value
            directionalSun.intensity = Mathf.MoveTowards(directionalSun.intensity, maxIntensity, Time.deltaTime);
        }
    }
}

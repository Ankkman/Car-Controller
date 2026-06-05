using UnityEngine;

public class BrakeLightController : MonoBehaviour
{
    [Header("Light Components")]
    public Light leftBrakeLight;
    public Light rightBrakeLight;

    [Header("Material Settings")]
    public Renderer carRenderer;
    public int brakeMaterialIndex = 13; 

    [Header("Intensity Settings")]
    public float lightOnIntensity = 20f; // Increased to 20 for URP visibility
    public float lightOffIntensity = 0f;

    private Material brakeMaterial;
    private BrakeSystem brakeSystem; // Reference to your physics brake script

    private readonly Color emissionOffColor = new Color(0.05f, 0f, 0f); 
    private readonly Color emissionOnColor = Color.red * 25f; // Multiplied by 25 for URP HDR glow

    void Start()
    {
        // Find the BrakeSystem script attached to this car
        brakeSystem = GetComponent<BrakeSystem>();

        // Cache the material instance safely
        if (carRenderer != null && brakeMaterialIndex < carRenderer.materials.Length)
        {
            brakeMaterial = carRenderer.materials[brakeMaterialIndex];
            brakeMaterial.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        CarController carController =
            GetComponent<CarController>();

        bool braking = false;

        // Space Brake
        if (Input.GetKey(KeyCode.Space))
            braking = true;

        // Handbrake
        if (Input.GetKey(KeyCode.LeftShift))
            braking = true;

        // D Mode + S
        if (
            carController != null &&
            carController.currentMode ==
            CarController.TransmissionMode.Drive &&
            Input.GetAxis("Vertical") < -0.1f
        )
        {
            braking = true;
        }

        // R Mode + W
        if (
            carController != null &&
            carController.currentMode ==
            CarController.TransmissionMode.Reverse &&
            Input.GetAxis("Vertical") > 0.1f
        )
        {
            braking = true;
        }

        float targetIntensity =
            braking ? lightOnIntensity : lightOffIntensity;

        if (leftBrakeLight != null)
            leftBrakeLight.intensity = targetIntensity;

        if (rightBrakeLight != null)
            rightBrakeLight.intensity = targetIntensity;

        if (brakeMaterial != null)
        {
            Color targetColor =
                braking ? emissionOnColor : emissionOffColor;

            brakeMaterial.SetColor(
                "_EmissionColor",
                targetColor
            );
        }
    }
}

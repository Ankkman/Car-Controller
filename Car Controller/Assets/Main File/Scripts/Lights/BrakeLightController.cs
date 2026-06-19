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
    private BrakeSystem brakeSystem; 
    private CarController carController; // Cached for performance

    private readonly Color emissionOffColor = new Color(0.05f, 0f, 0f); 
    private readonly Color emissionOnColor = Color.red * 25f; // Multiplied by 25 for URP HDR glow

    void Start()
    {
        // Cache our script references
        brakeSystem = GetComponent<BrakeSystem>();
        carController = GetComponent<CarController>();

        // Cache the material instance safely
        if (carRenderer != null && brakeMaterialIndex < carRenderer.materials.Length)
        {
            brakeMaterial = carRenderer.materials[brakeMaterialIndex];
            brakeMaterial.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        bool braking = false;

        if (carController != null)
        {
            // 1. Check if the vehicle is locked in Park mode
            if (carController.isParked)
            {
                braking = true;
            }
            // 2. Check if the PC or Mobile layout is currently passing brake pressure
            // (If useMobileInputs is active, MobileCarInput updates the brakeSystem component directly!)
            else if (brakeSystem != null && brakeSystem.brakeInput > 0.1f)
            {
                braking = true;
            }

        }

        // --- APPLY VISUAL CHANGES (Keep this exactly the same) ---
        float targetIntensity = braking ? lightOnIntensity : lightOffIntensity;

        if (leftBrakeLight != null)
            leftBrakeLight.intensity = targetIntensity;

        if (rightBrakeLight != null)
            rightBrakeLight.intensity = targetIntensity;

        if (brakeMaterial != null)
        {
            Color targetColor = braking ? emissionOnColor : emissionOffColor;
            brakeMaterial.SetColor("_EmissionColor", targetColor);
        }
    }

}

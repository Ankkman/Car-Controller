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
        // Default checking method (fallback if input script isn't found)
        bool braking = Input.GetKey(KeyCode.Space);

        // If the BrakeSystem script is found, check if ANY brake input is given
        if (brakeSystem != null)
        {
            // Read input states from your custom brake methods/variables
            // This works perfectly with the SetBrakeInput and SetHandbrakeInput system
            braking = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift); 
            
            // NOTE: If you use different keys for handbrake (e.g. Space for Handbrake, S for Normal Brake),
            // change 'KeyCode.LeftShift' to match your preferred secondary input key.
        }

        // 1. Control the Real Scene Spotlights
        float targetIntensity = braking ? lightOnIntensity : lightOffIntensity;
        if (leftBrakeLight != null) leftBrakeLight.intensity = targetIntensity;
        if (rightBrakeLight != null) rightBrakeLight.intensity = targetIntensity;

        // 2. Control the Visual Mesh Glow
        if (brakeMaterial != null)
        {
            Color targetColor = braking ? emissionOnColor : emissionOffColor;
            brakeMaterial.SetColor("_EmissionColor", targetColor);
        }
    }
}

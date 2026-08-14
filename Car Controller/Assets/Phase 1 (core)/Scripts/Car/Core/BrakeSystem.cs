using UnityEngine;
using System.Collections.Generic;

public class BrakeSystem : MonoBehaviour
{
    [Header("Main Brake Settings")]
    public float maxBrakeTorque = 15000f;
    public float frontBias = 0.70f;
    public float brakeRampSpeed = 25000f;

    [Header("Handbrake")]
    public float handbrakeTorque = 8000f;
    public float handbrakeRampSpeed = 15000f;

    [Header("ABS Settings")]
    public bool absEnabled = true;
    public float absSlipThreshold = 0.25f;   
    public float absReleaseRate = 0.05f;     
    public float absReapplyRate = 0.15f;     
    public float absMinBrakeTorque = 1000f;  

    [Header("Visual Lights (Spotlights)")]
    public Light leftBrakeLight;
    public Light rightBrakeLight;
    public float lightOnIntensity = 20f; 
    public float lightOffIntensity = 0f;

    [Header("Visual Mesh Glow (Materials)")]
    public Renderer carRenderer;
    public int brakeMaterialIndex = 13; 
    private Material brakeMaterial;
    private readonly Color emissionOffColor = new Color(0.1f, 0f, 0f); 
    private readonly Color emissionOnColor = Color.red * 25f;            

    [Header("Wheels References")]
    public List<WheelCollider> frontWheels;
    public List<WheelCollider> rearWheels;

    private Rigidbody rb;
    public float brakeInput; 
    private float handbrakeInput;
    private float currentBrakeTorque;
    private float currentHandbrakeTorque;

    // Reference to check Engine/Park state
    private CarController carController; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();

        // Cache and enable emission on your backlight material
        if (carRenderer != null && brakeMaterialIndex < carRenderer.materials.Length)
        {
            brakeMaterial = carRenderer.materials[brakeMaterialIndex];
            brakeMaterial.EnableKeyword("_EMISSION");
        }
    }

    public void SetBrakeInput(float input) => brakeInput = Mathf.Clamp01(input);
    public void SetHandbrakeInput(float input) => handbrakeInput = Mathf.Clamp01(input);

    void FixedUpdate()
    {
        bool engineIsOn = (carController != null && carController.engineOn);
        bool engineIsOff = !engineIsOn;
        
        // --- FIX: Brake lights ONLY turn on if the engine is actually running AND a brake is pressed ---
        bool isBrakingVisual = engineIsOn && (brakeInput > 0.01f || handbrakeInput > 0.01f);

        // 1. Control the Visual Spotlights
        float targetIntensity = isBrakingVisual ? lightOnIntensity : lightOffIntensity;
        if (leftBrakeLight != null) leftBrakeLight.intensity = targetIntensity;
        if (rightBrakeLight != null) rightBrakeLight.intensity = targetIntensity;

        // 2. Control the Visual Mesh Material Glow
        if (brakeMaterial != null)
        {
            // Force true darkness (black) when engine is off, or use emissionOffColor when engine is idling
            Color targetColor = isBrakingVisual ? emissionOnColor : (engineIsOn ? emissionOffColor : Color.black);
            brakeMaterial.SetColor("_EmissionColor", targetColor);
        }

        // --- PHYSICAL BRAKING FORCES REMAIN SAFELY LOCKED BELOW ---
        // If engine is off, we still keep the wheels physically frozen so it doesn't roll away
        bool isCarPhysicallyLocked = ((brakeInput > 0.01f || handbrakeInput > 0.01f) || engineIsOff);
        if (!isCarPhysicallyLocked)
        {
            currentBrakeTorque = 0f;
            currentHandbrakeTorque = 0f;
            foreach (var w in frontWheels) w.brakeTorque = 0f;
            foreach (var w in rearWheels) w.brakeTorque = 0f;
            return; 
        }

        float effectiveBrakeInput = engineIsOff ? 1f : brakeInput;

        // Ramp main brake torque
        float targetBrake = effectiveBrakeInput * maxBrakeTorque;
        currentBrakeTorque = Mathf.MoveTowards(currentBrakeTorque, targetBrake, brakeRampSpeed * Time.fixedDeltaTime);

        // Ramp handbrake torque
        float targetHandbrake = handbrakeInput * handbrakeTorque;
        currentHandbrakeTorque = Mathf.MoveTowards(currentHandbrakeTorque, targetHandbrake, handbrakeRampSpeed * Time.fixedDeltaTime);

        ApplyBrakes();
    }


    void ApplyBrakes()
    {
        float frontMain = currentBrakeTorque * frontBias;
        foreach (var w in frontWheels)
            ApplyABSToWheel(w, frontMain);

        float rearMain = currentBrakeTorque * (1f - frontBias);
        foreach (var w in rearWheels)
            ApplyABSToWheel(w, rearMain + currentHandbrakeTorque);
    }

    void ApplyABSToWheel(WheelCollider wheel, float desiredTorque)
    {
        float carSpeedMps = rb != null ? rb.linearVelocity.magnitude : 0f; 
        
        if (!absEnabled || carSpeedMps < 1.5f) 
        {
            wheel.brakeTorque = desiredTorque;
            return;
        }

        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            float slip = Mathf.Abs(hit.forwardSlip);

            if (slip > absSlipThreshold)
            {
                float newTorque = wheel.brakeTorque - (absReleaseRate * maxBrakeTorque * Time.fixedDeltaTime);
                wheel.brakeTorque = Mathf.Max(absMinBrakeTorque, newTorque);
            }
            else
            {
                float newTorque = wheel.brakeTorque + (absReapplyRate * maxBrakeTorque * Time.fixedDeltaTime);
                wheel.brakeTorque = Mathf.Min(desiredTorque, newTorque);
            }
        }
        else
        {
            wheel.brakeTorque = desiredTorque;
        }
    }
}

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
    public float lightOnIntensity = 20f; // Higher intensity for URP
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
    private float brakeInput;
    private float handbrakeInput;
    private float currentBrakeTorque;
    private float currentHandbrakeTorque;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

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
        // Check if either regular brake OR handbrake is actively being pressed
        bool isBrakingVisual = (brakeInput > 0.01f || handbrakeInput > 0.01f);

        // 1. Control the Visual Spotlights
        float targetIntensity = isBrakingVisual ? lightOnIntensity : lightOffIntensity;
        if (leftBrakeLight != null) leftBrakeLight.intensity = targetIntensity;
        if (rightBrakeLight != null) rightBrakeLight.intensity = targetIntensity;

        // 2. Control the Visual Mesh Material Glow
        if (brakeMaterial != null)
        {
            Color targetColor = isBrakingVisual ? emissionOnColor : emissionOffColor;
            brakeMaterial.SetColor("_EmissionColor", targetColor);
        }

        // Handle physical braking forces
        if (!isBrakingVisual)
        {
            currentBrakeTorque = 0f;
            currentHandbrakeTorque = 0f;
            
            foreach (var w in frontWheels) w.brakeTorque = 0f;
            foreach (var w in rearWheels) w.brakeTorque = 0f;
            
            return; 
        }

        // Ramp main brake torque
        float targetBrake = brakeInput * maxBrakeTorque;
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

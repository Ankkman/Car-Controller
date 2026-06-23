using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleHUD : MonoBehaviour
{
    [Header("References")]
    public Rigidbody carRigidbody;
    public Engine engine;
    public CarController carController;

    [Header("UI Text")]
    public TMP_Text speedText;
    public TMP_Text gearText;

    [Header("UI Backgrounds")]
    public Image gearBoxImage;

    [Header("Color Settings")]
    [Tooltip("How fast the gear panel and text transition to their new colors.")]
    public float colorTransitionSpeed = 4f;

    // Internal trackers for color interpolation
    private Color targetGearColor;
    private Color currentGearColor;
    private Color targetTextColor;
    private Color currentTextColor;

    // Custom softer green color (R: 140, G: 220, B: 140)
    private readonly Color softGreen = new Color(0.4f, 0.85f, 0.2f, 1f);

    [Header("Speed Needle")]
    public RectTransform speedNeedle;
    public float maxSpeed = 240f;
    public float minNeedleAngle = 140f;
    public float maxNeedleAngle = -140f;

    void Start()
    {
        // Initial clean dark variant baseline
        currentGearColor = new Color(0.12f, 0.15f, 0.2f, 0.85f);
        targetGearColor = currentGearColor;

        // Baseline text color (White)
        currentTextColor = Color.white;
        targetTextColor = currentTextColor;

        if (gearBoxImage != null)
        {
            gearBoxImage.color = currentGearColor;
        }

        if (gearText != null)
        {
            gearText.color = currentTextColor;
        }
    }

    void Update()
    {
        UpdateSpeed();
        UpdateGear();
        UpdateNeedle();
        ApplySmoothColorTransition();
    }

    void UpdateSpeed()
    {
        if (speedText == null) return;

        // --- IF ENGINE IS OFF: Show a single 0 ---
        if (carController != null && !carController.engineOn)
        {
            speedText.text = "0";
            speedText.color = Color.white;
            return;
        }

        float speedKmh = carRigidbody != null ? carRigidbody.linearVelocity.magnitude * 3.6f : 0f;
        int roundedSpeed = Mathf.RoundToInt(speedKmh);

        // Dynamic Speed Text Coloring
        string speedHexColor = "#FFFFFF"; // Clean white
        if (speedKmh > 200f) speedHexColor = "#FF0000";      // Redline
        else if (speedKmh > 160f) speedHexColor = "#FFD700"; // Warning yellow

        // --- UPDATED: No placeholder shadow digits, display active values directly ---
        speedText.text = $"<color={speedHexColor}>{roundedSpeed}</color>";
    }


    void UpdateGear()
    {
        if (carController == null || gearText == null) return;

        // --- FIXED: Clear out text and darken background when engine is off ---
        if (!carController.engineOn)
        {
            gearText.text = ""; 
            targetGearColor = new Color(0.08f, 0.08f, 0.08f, 0.85f); 
            targetTextColor = Color.clear; 
            return;
        }

        // Read active transmission data when the engine is running
        switch (carController.currentMode)
        {
            case CarController.TransmissionMode.Reverse:
                gearText.text = "R";
                targetGearColor = new Color(0.25f, 0.05f, 0.05f, 0.85f);
                targetTextColor = Color.red;
                break;

            case CarController.TransmissionMode.Neutral:
                gearText.text = "N";
                targetGearColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
                targetTextColor = Color.gray;
                break;

            case CarController.TransmissionMode.Drive:
                gearText.text = "D";
                targetGearColor = new Color(0.12f, 0.25f, 0.12f, 0.85f);
                targetTextColor = softGreen;
                break;

            case CarController.TransmissionMode.Park:
                gearText.text = "P";
                targetGearColor = new Color(0.05f, 0.15f, 0.25f, 0.85f);
                targetTextColor = Color.cyan;
                break;
        }
    }

    void UpdateNeedle()
    {
        if (speedNeedle == null) return;

        // --- FIXED: Drop dial immediately if engine cuts out ---
        if (carController != null && !carController.engineOn)
        {
            speedNeedle.localRotation = Quaternion.Euler(0, 0, minNeedleAngle);
            return;
        }

        float speedKmh = carRigidbody != null ? carRigidbody.linearVelocity.magnitude * 3.6f : 0f;
        float normalizedSpeed = Mathf.Clamp01(speedKmh / maxSpeed);
        float angle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, normalizedSpeed);
        speedNeedle.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void ApplySmoothColorTransition()
    {
        if (gearBoxImage != null)
        {
            currentGearColor = Color.Lerp(currentGearColor, targetGearColor, Time.deltaTime * colorTransitionSpeed);
            gearBoxImage.color = currentGearColor;
        }

        if (gearText != null)
        {
            currentTextColor = Color.Lerp(currentTextColor, targetTextColor, Time.deltaTime * colorTransitionSpeed);
            gearText.color = currentTextColor;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleHUD : MonoBehaviour
{
    [Header("References")]
    public Rigidbody carRigidbody;
    public Engine engine;

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

    public CarController carController;

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
        float speedKmh = carRigidbody.linearVelocity.magnitude * 3.6f;
        int roundedSpeed = Mathf.RoundToInt(speedKmh);
        speedText.text = roundedSpeed.ToString();

        // Speed text changes text color independently
        if (speedKmh > 200f) speedText.color = Color.red;
        else if (speedKmh > 160f) speedText.color = Color.yellow;
        else speedText.color = Color.white;
    }

    void UpdateGear()
    {
        switch (carController.currentMode)
        {
            case CarController.TransmissionMode.Reverse:

                gearText.text = "R";
                targetGearColor =
                    new Color(0.25f, 0.05f, 0.05f, 0.85f);
                targetTextColor = Color.red;
                break;

            case CarController.TransmissionMode.Neutral:

                gearText.text = "N";
                targetGearColor =
                    new Color(0.12f, 0.12f, 0.12f, 0.85f);
                targetTextColor = Color.gray;
                break;

            case CarController.TransmissionMode.Drive:

                gearText.text = "D";
                targetGearColor =
                    new Color(0.12f, 0.25f, 0.12f, 0.85f);
                targetTextColor = softGreen;
                break;

            case CarController.TransmissionMode.Park:

                gearText.text = "P";
                targetGearColor =
                    new Color(0.05f, 0.15f, 0.25f, 0.85f);
                targetTextColor = Color.cyan;
                break;
        }
    }

    void UpdateNeedle()
    {
        float speedKmh = carRigidbody.linearVelocity.magnitude * 3.6f;
        float normalizedSpeed = Mathf.Clamp01(speedKmh / maxSpeed);
        float angle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, normalizedSpeed);
        speedNeedle.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void ApplySmoothColorTransition()
    {
        // Smoothly transition background panel color
        if (gearBoxImage != null)
        {
            currentGearColor = Color.Lerp(currentGearColor, targetGearColor, Time.deltaTime * colorTransitionSpeed);
            gearBoxImage.color = currentGearColor;
        }

        // Smoothly transition gear text color
        if (gearText != null)
        {
            currentTextColor = Color.Lerp(currentTextColor, targetTextColor, Time.deltaTime * colorTransitionSpeed);
            gearText.color = currentTextColor;
        }
    }
}
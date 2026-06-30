using UnityEngine;  
using UnityEngine.UI;  
using TMPro; 

public class VehicleHUD : MonoBehaviour  
{  
    [Header("References")]  
    public Rigidbody carRigidbody;  
    public Engine engine;  
    public CarController carController;  
    public VehicleInputHandler inputHandler; 

    [Header("UI Text")]  
    public TMP_Text speedText;  
    public TMP_Text gearText; 

    [Header("UI Backgrounds")]  
    public Image gearBoxImage; 

    [Header("Color Settings")]  
    [Tooltip("How fast the gear panel and text transition to their new colors.")]  
    public float colorTransitionSpeed = 6f; 

    // Internal trackers for color interpolation  
    private Color targetGearColor;  
    private Color currentGearColor;  
    private Color targetTextColor;  
    private Color currentTextColor; 

    private readonly Color softGreen = new Color(0.4f, 0.85f, 0.2f, 1f); 

    [Header("Speed Needle")]  
    public RectTransform speedNeedle;  
    public float maxSpeed = 240f;  
    public float minNeedleAngle = 140f;  
    public float maxNeedleAngle = -140f; 

    [Header("Juice / Shift Animation")]  
    [Tooltip("How much the gear number grows when you shift.")]  
    public float shiftPunchScale = 1.35f;  
    [Tooltip("How fast the gear number shrinks back to its original size.")]  
    public float scaleSettleSpeed = 8f; 

    private string lastDisplayedGear = "";  
    private Vector3 originalGearScale; 

    // Runtime Dynamic UI reference link  
    private GameObject gearShiftButtonsPanel; 

    void Start()  
    {  
        currentGearColor = new Color(0.12f, 0.15f, 0.2f, 0.85f);  
        targetGearColor = currentGearColor; 

        currentTextColor = Color.white;  
        targetTextColor = currentTextColor; 

        if (gearBoxImage != null) gearBoxImage.color = currentGearColor; 

        if (gearText != null)  
        {  
            gearText.color = currentTextColor;  
            originalGearScale = gearText.transform.localScale;  
        } 

        FindGearShiftPanelReference();  
        FindAndLinkActiveCar();  
    } 

    void Update()  
    {  
        if (carController == null || inputHandler == null)  
        {  
            FindAndLinkActiveCar();  
        } 

        UpdateSpeed();  
        UpdateGear();  
        UpdateNeedle();  
        ApplySmoothColorTransition();  
        AnimateGearScale();  
    } 

    public void SetCar(GameObject newCar)  
    {  
        if (newCar == null) return; 

        carRigidbody = newCar.GetComponent<Rigidbody>();  
        engine = newCar.GetComponent<Engine>();  
        carController = newCar.GetComponent<CarController>();  
        inputHandler = newCar.GetComponent<VehicleInputHandler>(); 

        ToggleGearPanelVisibility();  
    } 

    private void FindAndLinkActiveCar()  
    {  
        GameObject activeCar = GameObject.FindGameObjectWithTag("Player");  
        if (activeCar != null)  
        {  
            SetCar(activeCar);  
        }  
    } 

    private void FindGearShiftPanelReference()  
    {  
        GameObject inGameUI = GameObject.FindGameObjectWithTag("InGameUI");  
        if (inGameUI != null)  
        {  
            // --- FIXED: Deep search recursively to find GearShiftPanel inside SteeringGroup ---  
            Transform foundTransform = FindChildWithName(inGameUI.transform, "GearShiftPanel");  
            if (foundTransform != null) gearShiftButtonsPanel = foundTransform.gameObject;  
        }  
    } 

    // Helper method to look deep into nested hierarchies  
    private Transform FindChildWithName(Transform parent, string targetName)  
    {  
        if (parent.name == targetName) return parent; 

        foreach (Transform child in parent)  
        {  
            Transform result = FindChildWithName(child, targetName);  
            if (result != null) return result;  
        }  
        return null;  
    } 

    private void ToggleGearPanelVisibility()  
    {  
        if (gearShiftButtonsPanel == null) FindGearShiftPanelReference(); 

        if (gearShiftButtonsPanel != null)  
        {  
            if (inputHandler != null)  
            {  
                gearShiftButtonsPanel.SetActive(!inputHandler.useAutomaticTransmission);  
            }  
            else  
            {  
                SettingsManager.LoadSettings();  
                gearShiftButtonsPanel.SetActive(!SettingsManager.IsAutomatic);  
            }  
        }  
    } 

    void UpdateSpeed()  
    {  
        if (speedText == null) return; 

        if (carController != null && !carController.engineOn)  
        {  
            speedText.text = "0";  
            speedText.color = Color.white;  
            return;  
        } 

        float speedKmh = carRigidbody != null ? carRigidbody.linearVelocity.magnitude * 3.6f : 0f;  
        int roundedSpeed = Mathf.RoundToInt(speedKmh); 

        string speedHexColor = "#FFFFFF";  
        if (speedKmh > 200f) speedHexColor = "#FF0000";  
        else if (speedKmh > 160f) speedHexColor = "#FFD700"; 

        speedText.text = $"<color={speedHexColor}>{roundedSpeed}</color>";  
    } 

    void UpdateGear()  
    {  
        if (carController == null || gearText == null) return; 

        if (!carController.engineOn)  
        {  
            UpdateGearTextAndCheckPunch("");  
            targetGearColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);  
            targetTextColor = Color.clear;  
            return;  
        } 

        switch (carController.currentMode)  
        {  
            case CarController.TransmissionMode.Reverse:  
                UpdateGearTextAndCheckPunch("R");  
                targetGearColor = new Color(0.25f, 0.05f, 0.05f, 0.85f);  
                targetTextColor = Color.red;  
                break; 

            case CarController.TransmissionMode.Neutral:  
                UpdateGearTextAndCheckPunch("N");  
                targetGearColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);  
                targetTextColor = Color.gray;  
                break; 

            case CarController.TransmissionMode.Drive:  
                if (inputHandler != null && !inputHandler.useAutomaticTransmission)  
                {  
                    int gearNum = inputHandler.CurrentManualGear;  
                    if (gearNum >= 1 && gearNum <= 6)  
                    {  
                        UpdateGearTextAndCheckPunch(gearNum.ToString());  
                    }  
                    else  
                    {  
                        UpdateGearTextAndCheckPunch("N");  
                    }  
                }  
                else  
                {  
                    UpdateGearTextAndCheckPunch("D");  
                } 

                targetGearColor = new Color(0.12f, 0.25f, 0.12f, 0.85f);  
                targetTextColor = softGreen;  
                break; 

            case CarController.TransmissionMode.Park:  
                UpdateGearTextAndCheckPunch("P");  
                targetGearColor = new Color(0.05f, 0.15f, 0.25f, 0.85f);  
                targetTextColor = Color.cyan;  
                break;  
        }  
    } 

    void UpdateGearTextAndCheckPunch(string newGearString)  
    {  
        if (gearText.text != newGearString)  
        {  
            gearText.text = newGearString;  
            if (!string.IsNullOrEmpty(newGearString) && !string.IsNullOrEmpty(lastDisplayedGear))  
            {  
                gearText.transform.localScale = originalGearScale * shiftPunchScale;  
            }  
            lastDisplayedGear = newGearString;  
        }  
    } 

    void AnimateGearScale()  
    {  
        if (gearText == null) return;  
        gearText.transform.localScale = Vector3.Lerp(gearText.transform.localScale, originalGearScale, Time.deltaTime * scaleSettleSpeed);  
    } 

    void UpdateNeedle()  
    {  
        if (speedNeedle == null) return; 

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

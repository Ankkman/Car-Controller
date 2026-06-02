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
    [Tooltip("How fast the gear panel transitions to the new color.")]
    public float colorTransitionSpeed = 4f;

    // Internal trackers for color interpolation
    private Color targetGearColor;
    private Color currentGearColor;

    void Start()
    {
        // Initial clean dark variant baseline
        currentGearColor = new Color(0.12f, 0.15f, 0.2f, 0.85f); 
        targetGearColor = currentGearColor;
        if (gearBoxImage != null)
        {
            gearBoxImage.color = currentGearColor;
        }
    }

    void Update() 
    { 
        UpdateSpeed(); 
        UpdateGear(); 
        ApplySmoothColorTransition();
    } 

    void UpdateSpeed() 
    { 
        float speedKmh = carRigidbody.linearVelocity.magnitude * 3.6f; 
        int roundedSpeed = Mathf.RoundToInt(speedKmh);
        speedText.text = roundedSpeed.ToString(); 

        // Speed text changes text color independently as requested
        if (speedKmh > 200f) speedText.color = Color.red;
        else if (speedKmh > 160f) speedText.color = Color.yellow;
        else speedText.color = Color.white;
    } 

    void UpdateGear() 
    { 
        float forwardSpeed = Vector3.Dot(carRigidbody.transform.forward, carRigidbody.linearVelocity);

        // 1. Reverse Logic
        if (forwardSpeed < -0.5f || (forwardSpeed < 0.1f && Input.GetAxis("Vertical") < -0.1f))
        {
            gearText.text = "R";
            targetGearColor = new Color(0.25f, 0.05f, 0.05f, 0.85f); // Deep muted red tint for reverse
        }
        // 2. Neutral Logic
        else if (forwardSpeed < 0.5f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.1f)
        {
            gearText.text = "N"; 
            targetGearColor = new Color(0.12f, 0.12f, 0.12f, 0.85f); // Plain flat dark gray
        }
        // 3. Drive Gears Logic (D1 - D4+)
        else 
        {
            int currentGearIndex = engine.CurrentGear; // 0 = First Gear, 1 = Second Gear, etc.
            gearText.text = "D" + (currentGearIndex + 1); 

            // Base color scheme: dark slate blue that grows progressively richer/brighter with gears
            switch (currentGearIndex)
            {
                case 0: // D1
                    targetGearColor = new Color(0.12f, 0.16f, 0.22f, 0.85f); // Dark Slate Blue
                    break;
                case 1: // D2
                    targetGearColor = new Color(0.14f, 0.20f, 0.30f, 0.85f); // Slightly Lighter Steel Blue
                    break;
                case 2: // D3
                    targetGearColor = new Color(0.16f, 0.25f, 0.38f, 0.85f); // Mid-tone Racing Blue
                    break;
                case 3: // D4
                default: // D5 and above
                    targetGearColor = new Color(0.18f, 0.30f, 0.48f, 0.85f); // Bright Vivid Navy
                    break;
            }
        }
    } 

    void ApplySmoothColorTransition()
    {
        if (gearBoxImage != null)
        {
            currentGearColor = Color.Lerp(currentGearColor, targetGearColor, Time.deltaTime * colorTransitionSpeed);
            gearBoxImage.color = currentGearColor;
        }
    }
}

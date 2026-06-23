using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSteeringWheel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Output")]
    public float steeringValue = 0f; // Clean -1 to 1 value for the car controller

    [Header("Settings")]
    public float rotationSpeed = 5f; // How fast the wheel snaps back to center
    public float maxRotationAngle = 200f; // Hard stop limit (in degrees) from center

    private RectTransform rectTransform;
    private bool isDragging = false;
    private float currentAngle = 0f;
    private float previousFingerAngle = 0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        
        // Calculate the initial finger angle when first touching the wheel
        Vector2 centerPosition = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rectTransform.position);
        Vector2 direction = eventData.position - centerPosition;
        previousFingerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Find where the center of the wheel is on your phone screen
        Vector2 centerPosition = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rectTransform.position);
        Vector2 direction = eventData.position - centerPosition;
        
        // Calculate current finger angle relative to center
        float currentFingerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Calculate how much the finger moved around the wheel since the last frame
        float angleDelta = Mathf.DeltaAngle(previousFingerAngle, currentFingerAngle);
        
        // Accumulate rotation and strictly clamp it between our minimum and maximum angles
        currentAngle += angleDelta;
        currentAngle = Mathf.Clamp(currentAngle, -maxRotationAngle, maxRotationAngle);
        
        // Save current angle for the next frame calculation
        previousFingerAngle = currentFingerAngle;

        // Apply physical visual rotation to the UI Image component
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    void Update()
    {
        // When the player releases the wheel, smoothly spring it back to center (0 degrees)
        if (!isDragging)
        {
            if (Mathf.Abs(currentAngle) > 0.5f)
            {
                currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * rotationSpeed);
                rectTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            }
            else
            {
                currentAngle = 0f;
                rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        // FIXED DIRECTION: Changed to negative calculation to flip the -1 to 1 value, 
        // ensuring turning left matches your vehicle setup perfectly.
        steeringValue = -Mathf.Clamp(currentAngle / maxRotationAngle, -1f, 1f);
    }
}

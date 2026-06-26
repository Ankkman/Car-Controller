using UnityEngine;
using UnityEngine.UI;

public class UIScreenSpaceArrow : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public MissionManager missionManager;
    public Image arrowImage; // Assign the UI Image component of your arrow here

    [Header("Settings")]
    public float rotationSpeed = 10f;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (arrowImage == null) arrowImage = GetComponent<Image>();
    }

    void LateUpdate()
    {
        if (missionManager == null || arrowImage == null || missionManager.ActiveTargetTransform == null)
        {
            if (arrowImage != null) arrowImage.enabled = false; // Hide if no active mission
            return;
        }

        arrowImage.enabled = true;
        Transform target = missionManager.ActiveTargetTransform;

        // Calculate direction relative to where the camera is looking
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
        
        // If the target is behind the camera, invert it so the pointer doesn't flip out
        if (screenPos.z < 0)
        {
            screenPos *= -1f;
        }

        // Project onto a flat 2D vector relative to the screen center
        Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 direction = screenPos - centerPos;
        direction.z = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            // Calculate 2D angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Adjust rotation so "Up" points forward (Unity UI default handles 0 degrees as Right)
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}

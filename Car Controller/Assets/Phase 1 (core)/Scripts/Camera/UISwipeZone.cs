using UnityEngine;
using UnityEngine.EventSystems;

// This script goes directly ON your invisible CameraSwipeZone UI Image object
public class UISwipeZone : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private FreeLookCamera freeLookCamera;

    void Start()
    {
        // Automatically find the main camera script in the scene
        freeLookCamera = FindFirstObjectByType<FreeLookCamera>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (freeLookCamera != null)
        {
            freeLookCamera.StartDragging(eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (freeLookCamera != null)
        {
            freeLookCamera.UpdateDragging(eventData.position);
        }
    }
}

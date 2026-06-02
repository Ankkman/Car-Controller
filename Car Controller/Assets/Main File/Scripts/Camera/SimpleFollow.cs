using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;          // Drag your main 'Chev' truck here

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 3f, -7f); // Height (Y) and Distance behind (Z)
    public float smoothSpeed = 5f;    // Higher number = faster tracking

    [Header("Rotation Settings")]
    public bool lookAtTarget = true;  // Keeps the target dead center in frame

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate the ideal spot behind the vehicle
        Vector3 targetPosition = target.TransformPoint(offset);

        // 2. Smoothly move from current position to that ideal spot
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 3. Keep looking right at the vehicle
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}

using UnityEngine; 

public class SimpleFollow : MonoBehaviour  
{  
    [Header("Target to Follow")]  
    public Transform target; // The active tracking transform 

    [Header("Position Settings")]  
    public Vector3 offset = new Vector3(0f, 3f, -7f); // Default baseline fallback height & distance  
    public float smoothSpeed = 5f; 

    [Header("Rotation Settings")]  
    public bool lookAtTarget = true; 

    void LateUpdate()  
    {  
        // SYSTEM RUNTIME LINK: If target went missing, find the fresh spawned car clone  
        if (target == null)  
        {  
            GameObject spawnedPlayer = GameObject.FindGameObjectWithTag("Player");  
            if (spawnedPlayer != null)  
            {  
                // --- SMART ANCHOR SEARCH ---  
                // Look inside the spawned prefab to see if it carries a custom camera anchor point  
                Transform customAnchor = spawnedPlayer.transform.Find("CameraLookTarget"); 

                if (customAnchor != null)  
                {  
                    target = customAnchor; // Lock onto the customized anchor point!  
                }  
                else  
                {  
                    target = spawnedPlayer.transform; // Fall back safely to vehicle root container  
                }  
            }  
            else  
            {  
                return; // Wait until the spawner finishes instantiating the vehicle asset  
            }  
        } 

        // 1. Calculate the ideal spot behind the vehicle anchor point  
        Vector3 targetPosition = target.TransformPoint(offset); 

        // 2. Smoothly move from current position to that ideal spot  
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime); 

        // 3. Keep looking right at the vehicle anchor  
        if (lookAtTarget)  
        {  
            Vector3 lookPoint = target.position + target.forward * 2f; 

            Quaternion targetRotation = Quaternion.LookRotation(lookPoint - transform.position); 

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);  
        }  
    }  
}

using UnityEngine;

public class MissionCheckpoint : MonoBehaviour
{
    private MissionManager manager;
    private GameObject spawnedIcon; // Tracks the minimap icon

    public void Initialize(MissionManager masterManager)
    {
        manager = masterManager;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // Assigns the icon instance to this specific checkpoint
    public void SetMinimapIcon(GameObject iconInstance)
    {
        spawnedIcon = iconInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarController>() != null)
        {
            if (manager != null)
            {
                // Clean up the icon right before destroying/deactivating
                if (spawnedIcon != null) Destroy(spawnedIcon); 
                
                manager.OnCheckpointReached(this);
            }
        }
    }
}

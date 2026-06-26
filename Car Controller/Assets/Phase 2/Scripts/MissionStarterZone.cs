using UnityEngine;

public class MissionStarterZone : MonoBehaviour
{
    [Header("References")]
    public MissionManager missionManager;
    
    [Header("UI Prompt (Optional)")]
    public GameObject interactionUIPanel;

    private bool playerInside = false;
    private GameObject myMinimapIcon; // Tracks the starter icon on the UI

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogWarning($"[MissionStarterZone] {gameObject.name} is missing a Collider component!");

        if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
    }

    // Called by the Manager to link the UI icon to this physical zone
    public void SetMinimapIcon(GameObject iconInstance)
    {
        myMinimapIcon = iconInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarController>() != null)
        {
            playerInside = true;
            if (interactionUIPanel != null) interactionUIPanel.SetActive(true);
            else StartAttachedMission();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarController>() != null)
        {
            playerInside = false;
            if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
        }
    }

    public void StartAttachedMission()
    {
        if (missionManager != null)
        {
            if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
            
            // Destroy the Start map icon the moment the mission is accepted
            if (myMinimapIcon != null) Destroy(myMinimapIcon);
            
            missionManager.StartMissionFromZone(this);
            gameObject.SetActive(false);
        }
    }
}
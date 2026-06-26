using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Flow Configuration")]
    public List<GameObject> missionCheckpoints = new List<GameObject>();
    public bool loopMissions = false;
    
    [Header("Open World Starters")]
    [Tooltip("Drag your MissionStarterZone objects here so they show up on the map at the start.")]
    public List<MissionStarterZone> activeStarterZones = new List<MissionStarterZone>();

    [Header("Dynamic Minimap Setup")]
    public GameObject startIconPrefab;       
    public GameObject checkpointIconPrefab;  
    public UIMinimap minimapCore;            
    public RectTransform minimapMask;        

    private int currentCheckpointIndex = 0;
    private MissionStarterZone activeStarterZone; 
    private GameObject currentActiveIcon; 

    public Transform ActiveTargetTransform { get; private set; }

    void Start()
    {
        // 1. Hide all mission checkpoints at start
        foreach (var cp in missionCheckpoints)
        {
            if (cp != null) cp.SetActive(false);
        }

        // 2. Spawn icons for all active starter zones the moment the game loads
        foreach (var starter in activeStarterZones)
        {
            if (starter != null && starter.gameObject.activeInHierarchy)
            {
                GameObject startIcon = SpawnBlip(startIconPrefab, starter.transform);
                starter.SetMinimapIcon(startIcon);
            }
        }
    }

    // Helper Method to spawn icons cleanly
    private GameObject SpawnBlip(GameObject prefab, Transform target)
    {
        if (minimapCore == null || minimapMask == null || prefab == null) return null;
        
        GameObject newIcon = Instantiate(prefab, minimapMask);
        MinimapBlip blipScript = newIcon.GetComponent<MinimapBlip>();
        if (blipScript != null)
        {
            blipScript.Setup(target, minimapCore, minimapMask);
        }
        return newIcon;
    }

    public void StartMissionFromZone(MissionStarterZone starterZone)
    {
        activeStarterZone = starterZone;
        InitializeMissionFlow();
    }

    void InitializeMissionFlow()
    {
        if (missionCheckpoints == null || missionCheckpoints.Count == 0) return;

        currentCheckpointIndex = 0;

        for (int i = 0; i < missionCheckpoints.Count; i++)
        {
            if (missionCheckpoints[i] != null)
            {
                MissionCheckpoint checkpointScript = missionCheckpoints[i].GetComponent<MissionCheckpoint>();
                if (checkpointScript == null)
                    checkpointScript = missionCheckpoints[i].AddComponent<MissionCheckpoint>();

                checkpointScript.Initialize(this);
                missionCheckpoints[i].SetActive(false); 
            }
        }

        ActivateCheckpoint(currentCheckpointIndex);
    }

    void ActivateCheckpoint(int index)
    {
        if (index >= 0 && index < missionCheckpoints.Count && missionCheckpoints[index] != null)
        {
            missionCheckpoints[index].SetActive(true);
            ActiveTargetTransform = missionCheckpoints[index].transform;

            // Use the helper method to spawn ONLY the checkpoint prefab
            currentActiveIcon = SpawnBlip(checkpointIconPrefab, missionCheckpoints[index].transform);

            // Link to checkpoint for cleanup
            MissionCheckpoint cpScript = missionCheckpoints[index].GetComponent<MissionCheckpoint>();
            if (cpScript != null)
            {
                cpScript.SetMinimapIcon(currentActiveIcon);
            }
        }
    }

    public void OnCheckpointReached(MissionCheckpoint checkpoint)
    {
        if (missionCheckpoints[currentCheckpointIndex] == checkpoint.gameObject)
        {
            checkpoint.gameObject.SetActive(false);
            currentCheckpointIndex++;

            if (currentCheckpointIndex < missionCheckpoints.Count)
            {
                ActivateCheckpoint(currentCheckpointIndex);
            }
            else
            {
                OnAllCheckpointsComplete();
            }
        }
    }

    void OnAllCheckpointsComplete()
    {
        ActiveTargetTransform = null;
        Debug.Log("<color=lime>[Mission Completed] All destination objectives met!</color>");

        if (activeStarterZone != null)
        {
            activeStarterZone.gameObject.SetActive(true);
            
            // Respawn the start icon so the player can play the mission again
            GameObject startIcon = SpawnBlip(startIconPrefab, activeStarterZone.transform);
            activeStarterZone.SetMinimapIcon(startIcon);
        }

        if (loopMissions)
        {
            InitializeMissionFlow();
        }
    }
}
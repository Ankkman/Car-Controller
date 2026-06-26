using UnityEngine;

public class UIMinimap : MonoBehaviour
{
    [Header("References")]
    public Transform playerTarget;     
    public RectTransform mapGraphic;   
    public RectTransform playerBlip;   

    [Header("Map Settings")]
    public Vector2 worldMapSize = new Vector2(933.014f, 633.023f); 
    public Vector2 worldOffset = new Vector2(0.001998901f, 0.002502441f);
    
    [Header("Calibration")]
    [Range(0.1f, 2f)]
    public float trackingScale = 0.92f;

    // Toggled by your MinimapExpander script
    [HideInInspector] public bool isExpandedMode = false; 

    void Update()
    {
        UpdateMapPosition();
        UpdatePlayerRotation();
    }

    private void UpdateMapPosition()
    {
        // Absolute normalized position of the car on the map
        float normalizedX = (playerTarget.position.x - worldOffset.x) / worldMapSize.x;
        float normalizedZ = (playerTarget.position.z - worldOffset.y) / worldMapSize.y;

        float uiX = normalizedX * (mapGraphic.sizeDelta.x * trackingScale);
        float uiY = normalizedZ * (mapGraphic.sizeDelta.y * trackingScale);

        if (!isExpandedMode)
        {
            // NORMAL MINIMAP: Map slides opposite to car, car stays locked in center
            mapGraphic.anchoredPosition = new Vector2(-uiX, -uiY);
            playerBlip.anchoredPosition = Vector2.zero;
        }
        else
        {
            // FULL MAP: Map locks to exact center, car blip moves to its true coordinate
            mapGraphic.anchoredPosition = Vector2.zero;
            playerBlip.anchoredPosition = new Vector2(uiX, uiY);
        }
    }

    private void UpdatePlayerRotation()
    {
        Vector3 carEuler = playerTarget.eulerAngles;
        playerBlip.localEulerAngles = new Vector3(0, 0, -carEuler.y);
    }
}
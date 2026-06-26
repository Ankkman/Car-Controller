using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [Header("References")]
    public RawImage minimapRawImage;       // The raw image showing camera feed
    public RectTransform playerArrow;       // Player direction indicator
    public Transform carTarget;             // The car transform
    public Camera minimapCamera;            // Reference to the minimap camera

    [Header("Player Arrow Settings")]
    public float arrowSize = 20f;           // UI size of player arrow
    public Color playerColor = Color.white;
    public bool showDirection = true;       // Rotate arrow to show car direction

    [Header("Border Settings")]
    public RectTransform mapBorder;         // Optional circular border frame
    public float mapSize = 200f;            // Size of minimap in pixels

    private RectTransform minimapRect;
    private Image arrowImage;

    void Start()
    {
        if (minimapRawImage != null)
            minimapRect = minimapRawImage.GetComponent<RectTransform>();

        if (playerArrow != null)
            arrowImage = playerArrow.GetComponent<Image>();

        // Set minimap size
        if (minimapRect != null)
            minimapRect.sizeDelta = new Vector2(mapSize, mapSize);

        if (mapBorder != null)
            mapBorder.sizeDelta = new Vector2(mapSize + 10, mapSize + 10); // Slightly larger border
    }

    void Update()
    {
        if (carTarget == null || minimapCamera == null || playerArrow == null)
            return;

        UpdatePlayerArrowPosition();
    }

    void UpdatePlayerArrowPosition()
    {
        // Convert world position to viewport position (0 to 1)
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(carTarget.position);

        // Convert viewport to minimap local position
        // Viewport (0,0) = bottom-left of camera, (1,1) = top-right
        float xPos = (viewportPos.x - 0.5f) * minimapRect.sizeDelta.x;
        float yPos = (viewportPos.y - 0.5f) * minimapRect.sizeDelta.y;

        playerArrow.localPosition = new Vector3(xPos, yPos, 0f);

        // Rotate arrow to show car direction (only Y-axis rotation matters for top-down)
        if (showDirection)
        {
            float carYRotation = carTarget.eulerAngles.y;
            playerArrow.localRotation = Quaternion.Euler(0, 0, -carYRotation); // Negative for UI rotation
        }
    }

    // Public method to add custom waypoints/icons
    public RectTransform CreateMapIcon(Sprite iconSprite, Color color, Vector3 worldPosition)
    {
        GameObject iconObj = new GameObject("MapIcon");
        iconObj.transform.SetParent(transform);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = color;
        
        iconRect.sizeDelta = new Vector2(15, 15); // Small icon size
        
        // Position will need to be updated in Update() or a separate script
        return iconRect;
    }
}
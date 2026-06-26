using UnityEngine;

public class MinimapBlip : MonoBehaviour
{
    private Transform target3D;
    private UIMinimap minimapCore;
    private RectTransform maskRect;
    private RectTransform blipRect;

    private float edgePadding = 15f; 

    public void Setup(Transform worldTarget, UIMinimap core, RectTransform mask)
    {
        target3D = worldTarget;
        minimapCore = core;
        maskRect = mask;
        blipRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target3D == null || minimapCore == null) return;

        if (!minimapCore.isExpandedMode)
        {
            // MINIMAP MODE: Position relative to player + Edge Clamping
            Vector3 offset = target3D.position - minimapCore.playerTarget.position;
            float normalizedX = offset.x / minimapCore.worldMapSize.x;
            float normalizedZ = offset.z / minimapCore.worldMapSize.y;

            float uiX = normalizedX * (minimapCore.mapGraphic.sizeDelta.x * minimapCore.trackingScale);
            float uiY = normalizedZ * (minimapCore.mapGraphic.sizeDelta.y * minimapCore.trackingScale);
            Vector2 targetPos = new Vector2(uiX, uiY);

            float maxAllowedX = (maskRect.rect.width / 2f) - edgePadding;
            float maxAllowedY = (maskRect.rect.height / 2f) - edgePadding;

            float boundRatioX = Mathf.Abs(targetPos.x / maxAllowedX);
            float boundRatioY = Mathf.Abs(targetPos.y / maxAllowedY);
            float maxBound = Mathf.Max(boundRatioX, boundRatioY);

            if (maxBound > 1f) targetPos /= maxBound;

            blipRect.anchoredPosition = targetPos;
        }
        else
        {
            // FULL MAP MODE: Absolute position on the static map, NO edge clamping
            Vector3 absoluteOffset = target3D.position - new Vector3(minimapCore.worldOffset.x, 0, minimapCore.worldOffset.y);
            float normalizedX = absoluteOffset.x / minimapCore.worldMapSize.x;
            float normalizedZ = absoluteOffset.z / minimapCore.worldMapSize.y;

            float uiX = normalizedX * (minimapCore.mapGraphic.sizeDelta.x * minimapCore.trackingScale);
            float uiY = normalizedZ * (minimapCore.mapGraphic.sizeDelta.y * minimapCore.trackingScale);
            
            blipRect.anchoredPosition = new Vector2(uiX, uiY);
        }

        blipRect.localRotation = Quaternion.identity;
    }
}
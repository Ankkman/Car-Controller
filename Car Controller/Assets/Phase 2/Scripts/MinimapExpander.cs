using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapExpander : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public UIMinimap minimapCore;       // NEW: Link to the core math script
    public RectTransform minimapCase;   
    public RectTransform minimapMask;   
    public GameObject closeOverlay;     

    [Header("Expanded Settings")]
    public Vector2 expandedMaskSize = new Vector2(800f, 800f); 
    public Vector2 expandedCaseSize = new Vector2(820f, 820f); 
    
    private Vector2 centerPosition = Vector2.zero; 

    private Vector2 normalMaskSize;
    private Vector2 normalCaseSize;
    private Vector2 normalPosition;
    private Vector2 normalMinAnchor; 
    private Vector2 normalMaxAnchor; 
    private Vector2 normalPivot;      
    private int originalSiblingIndex;
    private bool isExpanded = false;

    void Start()
    {
        normalCaseSize = minimapCase.sizeDelta;
        normalMaskSize = minimapMask.sizeDelta;
        normalPosition = minimapCase.anchoredPosition;
        
        normalMinAnchor = minimapCase.anchorMin;
        normalMaxAnchor = minimapCase.anchorMax;
        normalPivot = minimapCase.pivot;

        if(closeOverlay != null) closeOverlay.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isExpanded) ExpandMap();
    }

    public void ExpandMap()
    {
        isExpanded = true;
        if (minimapCore != null) minimapCore.isExpandedMode = true; // Tell the map to lock

        if(closeOverlay != null) closeOverlay.SetActive(true);

        originalSiblingIndex = minimapCase.GetSiblingIndex();
        
        if (closeOverlay != null) closeOverlay.transform.SetAsLastSibling();
        minimapCase.SetAsLastSibling(); 

        minimapCase.anchorMin = new Vector2(0.5f, 0.5f);
        minimapCase.anchorMax = new Vector2(0.5f, 0.5f);
        minimapCase.pivot = new Vector2(0.5f, 0.5f);
        minimapCase.anchoredPosition = centerPosition;
        
        minimapCase.sizeDelta = expandedCaseSize;
        minimapMask.sizeDelta = expandedMaskSize;
    }

    public void ShrinkMap()
    {
        isExpanded = false;
        if (minimapCore != null) minimapCore.isExpandedMode = false; // Tell the map to track again

        if(closeOverlay != null) closeOverlay.SetActive(false);

        minimapCase.anchorMin = normalMinAnchor;
        minimapCase.anchorMax = normalMaxAnchor;
        minimapCase.pivot = normalPivot;
        minimapCase.anchoredPosition = normalPosition;

        minimapCase.sizeDelta = normalCaseSize;
        minimapMask.sizeDelta = normalMaskSize;

        minimapCase.SetSiblingIndex(originalSiblingIndex);
    }
}
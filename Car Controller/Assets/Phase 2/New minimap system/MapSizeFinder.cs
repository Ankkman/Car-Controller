using UnityEngine;

public class MapSizeFinder : MonoBehaviour
{
    [Header("Drag Your 4 Custom Reference Cubes Here")]
    public Transform cubeLeft;
    public Transform cubeRight;
    public Transform cubeTop;
    public Transform cubeBottom;

    [ContextMenu("Calculate Map Settings From Cubes")]
    public void CalculateMapSettingsFromCubes()
    {
        if (cubeLeft == null || cubeRight == null || cubeTop == null || cubeBottom == null)
        {
            Debug.LogError("Missing reference! Please drag all 4 directional cubes into their respective inspector slots.");
            return;
        }

        // Calculate size: Right - Left for X width, Top - Bottom for Z length
        float totalWidthX = cubeRight.position.x - cubeLeft.position.x;
        float totalLengthZ = cubeTop.position.z - cubeBottom.position.z;

        // Calculate offset: Midpoint average equations to locate true mathematical center
        float centerX = (cubeRight.position.x + cubeLeft.position.x) / 2f;
        float centerZ = (cubeTop.position.z + cubeBottom.position.z) / 2f;

        // Output absolute clean values formatted for the target script
        Debug.LogWarning($"=== NEW MINIMAP SETTINGS PROCESSED ===");
        Debug.LogWarning($"worldMapSize = new Vector2({Mathf.Abs(totalWidthX)}f, {Mathf.Abs(totalLengthZ)}f);");
        Debug.LogWarning($"worldOffset = new Vector2({centerX}f, {centerZ}f);");
        Debug.LogWarning($"======================================");
    }
}

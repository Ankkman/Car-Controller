using UnityEngine;

public class TreadGenerator : MonoBehaviour
{
    [ContextMenu("Generate Skid Texture")]
    void GenerateTexture()
    {
        // Create a small 64x64 texture
        Texture2D texture = new Texture2D(64, 64);
        Color darkGrey = new Color(0.15f, 0.15f, 0.15f, 1f);
        Color transparent = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                // Create two distinct parallel tire track lines with a gap in the middle
                if ((x >= 8 && x <= 24) || (x >= 40 && x <= 56))
                {
                    // Create horizontal tread grooves every 16 pixels
                    if (y % 16 < 4) 
                        texture.SetPixel(x, y, transparent);
                    else 
                        texture.SetPixel(x, y, darkGrey);
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        texture.Apply();
        
        // Save the image out to your assets folder
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/TireTread.png", bytes);
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
        Debug.Log("Tire tread generated successfully!");
    }
}

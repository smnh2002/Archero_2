using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AutoFixBackground
{
    static AutoFixBackground()
    {
        string[] paths = {
            "Assets/Pictures/Lose 2.png",
            "Assets/Pictures/Victory 2.png"
        };
        foreach (string path in paths) {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                Debug.Log(path + " has been imported as a Sprite.");
            }
        }
    }
}

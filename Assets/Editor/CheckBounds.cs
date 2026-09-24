using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class CheckBounds
{
    static CheckBounds()
    {
        string path = "Assets/New assets/PurePoly/Mining_Pack/Prefabs/Environment/PP_Stone_Ground_01.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
            Bounds b = new Bounds();
            if (renderers.Length > 0)
            {
                b = renderers[0].bounds;
                for (int i=1; i<renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            }
            File.WriteAllText("Assets/bounds.txt", "Ground Size: " + b.size.ToString());
        }
    }
}

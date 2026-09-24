using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[InitializeOnLoad]
public class FixGroundLines
{
    static FixGroundLines()
    {
        EditorApplication.delayCall += () =>
        {
            var rings = new List<GameObject>();
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var go in allObjects)
            {
                if (go.name.Contains("FX_Mystic Ring") || go.name.Contains("FX_Mystic_Shine"))
                {
                    rings.Add(go);
                }
            }

            if (rings.Count > 0)
            {
                foreach (var r in rings)
                {
                    Object.DestroyImmediate(r);
                }
                Debug.Log($"[Antigravity] Zemin üzerindeki {rings.Count} adet parlayan mavi çizgi / çember temizlendi.");
            }
        };
    }
}
#endif

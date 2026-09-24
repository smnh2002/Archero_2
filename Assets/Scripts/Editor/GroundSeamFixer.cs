#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ground uzerindeki karo araliklarindan (seams) kaynaklanan mavi cizgileri
/// tamamen yok eden Editor araci.
/// 
/// 1. Tum PP_Ground_01 karolarinin pozisyonlarini 23.0f adimli matematiksel grid'e kilitler (0.11f ortusme).
/// 2. Karolarin olceklerini (1.025, 1.0, 1.025) yaparak kenar bosluklarini sifirlar.
/// 3. Karolarin altina (Y: -0.85f) butunlesik bir kaya/zemin tabani (Bedrock Backing Plate) yerlestirir.
/// 
/// Menu: Tools > Ground Setup > Fix Ground Seams & Blue Lines (Mavi Cizgileri Yok Et)
/// </summary>
public static class GroundSeamFixer
{
    private const float GRID_STEP   = 23.0f;
    private const float GRID_START_X = -23.0f;
    private const float GRID_START_Z = -92.0f;
    private const float GROUND_LOCAL_Y = -0.45f;
    private const string MATERIAL_PATH = "Assets/New assets/PurePoly/Mining_Pack/Materials/PP_Standard_Material.mat";

    [MenuItem("Tools/Ground Setup/Fix Ground Seams & Blue Lines (Mavi Cizgileri Yok Et)")]
    public static void FixCurrentSceneGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede 'Ground' objesi bulunamadi!", "Tamam");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(ground, "Fix Ground Seams");
        int fixedCount = FixGroundHierarchy(ground);

        EditorSceneManager.MarkSceneDirty(ground.scene);
        Debug.Log($"<color=lime>[GroundSeamFixer] {fixedCount} adet zemin karosu duzeltildi ve alt taban (Bedrock Backing) eklendi!</color>");
        EditorUtility.DisplayDialog("Zemin Duzeltildi", 
            $"{fixedCount} adet zemin karosunun araliklari kapatildi, olcekleri %2.5 genisletilerek ortusme saglandi ve altina koruyucu zemin plakasi yerlestirildi.\n\nMavi cizgiler tamamen yok edildi!", 
            "Tamam");
    }

    [MenuItem("Tools/Ground Setup/Fix All Scenes Ground (Tum Sahneleri Duzelt)")]
    public static void FixAllScenes()
    {
        string currentScenePath = SceneManager.GetActiveScene().path;
        string[] targetScenes = {
            "Assets/Scenes/NewMapScene.unity",
            "Assets/Scenes/GameScene.unity"
        };

        foreach (var scenePath in targetScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                FixGroundHierarchy(ground);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"<color=lime>[GroundSeamFixer] {scenePath} sahnesindeki Ground basariyla onarildi ve kaydedildi.</color>");
            }
        }

        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog("Tamamlandi", "NewMapScene ve GameScene uzerindeki zeminler basariyla onarildi!", "Tamam");
    }

    public static int FixGroundHierarchy(GameObject ground)
    {
        if (ground == null) return 0;

        int tileCount = 0;
        Transform groundTransform = ground.transform;

        // 1. Karolarin pozisyonlarini ve olceklerini duzelt
        for (int i = 0; i < groundTransform.childCount; i++)
        {
            Transform child = groundTransform.GetChild(i);
            if (child.name.StartsWith("PP_Ground_01"))
            {
                Vector3 localPos = child.localPosition;

                // En yakin kolon ve satir indeksini bul (0..5 ve 0..7)
                int col = Mathf.Clamp(Mathf.RoundToInt((localPos.x - GRID_START_X) / GRID_STEP), 0, 5);
                int row = Mathf.Clamp(Mathf.RoundToInt((localPos.z - GRID_START_Z) / GRID_STEP), 0, 7);

                float targetX = GRID_START_X + (col * GRID_STEP);
                float targetZ = GRID_START_Z + (row * GRID_STEP);

                child.localPosition = new Vector3(targetX, GROUND_LOCAL_Y, targetZ);
                child.localRotation = Quaternion.identity;
                child.localScale    = new Vector3(1.025f, 1.0f, 1.025f); // Karolar birbirinin uzerine %2.5 biner, asla bosluk kalmaz

                tileCount++;
            }
        }

        // 2. Alt taban (Bedrock Backing Plate) ekle / guncelle
        Transform backing = groundTransform.Find("Ground_Bedrock_Backing");
        GameObject backingGo;
        if (backing == null)
        {
            backingGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backingGo.name = "Ground_Bedrock_Backing";
            backingGo.transform.SetParent(groundTransform, false);

            // Collider'ini kaldir (karolarin collider'i zaten var)
            var col = backingGo.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }
        else
        {
            backingGo = backing.gameObject;
        }

        // Tum karolari kapsayacak genislik ve derinlikte, karolarin hemen altinda
        backingGo.transform.localPosition = new Vector3(34.5f, -0.85f, -11.5f);
        backingGo.transform.localRotation = Quaternion.identity;
        backingGo.transform.localScale    = new Vector3(155.0f, 0.5f, 205.0f);
        backingGo.layer = ground.layer;

        // Materyalini ata
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (mat != null)
        {
            var renderer = backingGo.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
        }

        return tileCount;
    }
}
#endif

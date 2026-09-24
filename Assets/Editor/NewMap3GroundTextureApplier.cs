using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// NewMap3Scene'deki tüm PP_Ground_01 prefab instancelarının materyalini
/// NewMap3_Ground.mat (NewMap3picture.png texture'lı) ile değiştirir.
/// Menu: Tools -> Apply NewMap3 Ground Texture
/// </summary>
public class NewMap3GroundTextureApplier : EditorWindow
{
    private const string MATERIAL_PATH =
        "Assets/New assets/PurePoly/Mining_Pack/Materials/NewMap3_Ground.mat";

    [MenuItem("Tools/Apply NewMap3 Ground Texture")]
    public static void ApplyTexture()
    {
        // Sahne kontrolü
        if (SceneManager.GetActiveScene().name != "NewMap3Scene")
        {
            if (EditorUtility.DisplayDialog("Yanlış Sahne",
                "'NewMap3Scene' sahnesinde çalıştırmalısınız. Şimdi açılsın mı?",
                "Evet", "Hayır"))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/NewMap3Scene.unity");
            }
            else return;
        }

        // Materyali yükle
        Material groundMat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (groundMat == null)
        {
            Debug.LogError("[NewMap3Ground] Materyal bulunamadı: " + MATERIAL_PATH +
                           "\nLütfen önce materyal dosyasının var olduğundan emin olun.");
            return;
        }

        // Sahnedeki tüm MeshRenderer'ları tara
        MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(
            FindObjectsSortMode.None);

        int changed = 0;

        foreach (var mr in allRenderers)
        {
            // PP_Ground_01 instancelarını isimle tanı
            if (!mr.gameObject.name.StartsWith("PP_Ground_01")) continue;

            // Materyali değiştir (shared olarak, prefab bağlantısını korur)
            Material[] mats = mr.sharedMaterials;
            bool dirty = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != groundMat)
                {
                    mats[i] = groundMat;
                    dirty = true;
                }
            }
            if (dirty)
            {
                mr.sharedMaterials = mats;
                changed++;
            }
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[NewMap3Ground] ✅ {changed} adet PP_Ground_01 objesine NewMap3_Ground materyali uygulandı.");
        }
        else
        {
            Debug.LogWarning("[NewMap3Ground] Hiçbir PP_Ground_01 objesi bulunamadı veya zaten güncel.");
        }
    }
}

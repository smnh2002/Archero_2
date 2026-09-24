#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// NewMapScene ground sinirlari icinde assetleri normal boyutlarinda (scale ~1.0)
/// ve birbirini kapatmayacak sekilde aralikli olarak dagitan Editor araci.
/// 
/// Menu: Tools > Mine Dungeon Builder > Scatter Mine Assets (Random Grid)
/// </summary>
public static class AbandonedMineSceneBuilder
{
    // ── Ground Sinirlari ─────────────────────────────────────────────────
    private const float GROUND_MIN_X = -18.0f;
    private const float GROUND_MAX_X =  88.0f;
    private const float GROUND_MIN_Z = -86.0f;
    private const float GROUND_MAX_Z =  64.0f;

    private const string BASE = "Assets/New assets/PurePoly/Mining_Pack/Prefabs";
    private const string ROOT_NAME = "[Abandoned_Mine_Scene]";

    [MenuItem("Tools/Mine Dungeon Builder/Scatter Mine Assets (Normal Size & Well Spaced)")]
    public static void ScatterMineAssets()
    {
        if (!EditorUtility.DisplayDialog("Maden Assetlerini Dagit",
            "Maden assetleri normal boyutlarinda (1.0x) ve birbirini kapatmayacak sekilde ground uzerine dagitilacak.\n\n" +
            "Devasa kayalar olmayacak, her sey net gorulebilecek.\nDevam edilsin mi?",
            "Evet, Dagit", "Iptal"))
            return;

        Undo.SetCurrentGroupName("Scatter Mine Assets");
        int group = Undo.GetCurrentGroup();

        ClearOldMineObjects(false);

        GameObject root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Create Mine Root");

        // Kullanılacak zengin asset havuzu (hepsi normal 1.0x boyutta)
        List<string> assetPool = new List<string>
        {
            // ── 1. Maden Arabaları ve Raylar ──
            BASE + "/Rails and Mine Carts/PP_Mine_Cart_01.prefab",
            BASE + "/Rails and Mine Carts/PP_Mine_Cart_02.prefab",
            BASE + "/Rails and Mine Carts/PP_Mine_Cart_04.prefab",
            BASE + "/Rails and Mine Carts/PP_Mine_Cart_05.prefab",
            BASE + "/Rails and Mine Carts/PP_Mine_Cart_08.prefab",
            BASE + "/Rails and Mine Carts/PP_Rail_Straight_01.prefab",
            BASE + "/Rails and Mine Carts/PP_Rail_Straight_Long.prefab",
            BASE + "/Rails and Mine Carts/PP_Rail_Curved_01.prefab",
            BASE + "/Rails and Mine Carts/PP_Wooden_Blockade_01.prefab",

            // ── 2. Ahşap Maden Destek Kirişleri ve Portallar ──
            BASE + "/Rails and Mine Carts/PP_Main_Entrance_01_Wooden.prefab",
            BASE + "/Cave/PP_Mine_Wooden_Support_01.prefab",
            BASE + "/Cave/PP_Mine_Wooden_Support_02.prefab",
            BASE + "/Cave/PP_Mine_Wooden_Support_03.prefab",
            BASE + "/Cave/PP_Mine_Wooden_Support_04.prefab",

            // ── 3. Kristaller ve Damarlar ──
            BASE + "/Ores and Crystals/PP_Crystal_Cluster_01_Blue.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Cluster_02_Blue.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Cluster_01_Red.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Cluster_01_Gold.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Cluster_01_Green.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Column_01_Blue.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Column_01_Red.prefab",
            BASE + "/Ores and Crystals/PP_Crystal_Column_01_Green.prefab",

            // ── 4. Madenci Eşyaları (Çadır, Ateş, Sandık, Fıçı) ──
            BASE + "/Props/PP_Tent_01.prefab",
            BASE + "/Props/PP_Tent_03.prefab",
            BASE + "/Props/PP_Campfire_01.prefab",
            BASE + "/Props/PP_Campfire_02.prefab",
            BASE + "/Props/PP_Treasure_Chest_01_Gold.prefab",
            BASE + "/Props/PP_Treasure_Chest_01_Iron.prefab",
            BASE + "/Props/PP_Treasure_Chest_01_Silver.prefab",
            BASE + "/Props/PP_Barrel_01.prefab",
            BASE + "/Props/PP_Barrel_03.prefab",
            BASE + "/Props/PP_Crate_Wooden_01.prefab",
            BASE + "/Props/PP_Crate_Wooden_02.prefab",
            BASE + "/Props/PP_Log_Pile_01.prefab",
            BASE + "/Props/PP_Log_Pile_02.prefab",
            BASE + "/Props/PP_Furnace_01.prefab",
            BASE + "/Props/PP_Wooden_Table_01.prefab",
            BASE + "/Props/PP_Dynamite_Bundle_01.prefab",
            BASE + "/Props/PP_Dynamite_Bundle_02.prefab",
            BASE + "/Props/PP_Tool_Holder_01.prefab",

            // ── 5. Rün Taşları, Sütunlar ve Kafatasları ──
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_01.prefab",
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_02.prefab",
            BASE + "/Pillars, Runes/PP_Rune_Stone_01.prefab",
            BASE + "/Pillars, Runes/PP_Rune_Stone_02.prefab",
            BASE + "/Cave/PP_Cave_Skull_01.prefab",
            BASE + "/Cave/PP_Cave_Skull_02.prefab",

            // ── 6. Normal Boyutta Kayalar ve Dikitler (Devasa OLMAYAN) ──
            BASE + "/Stones, Rocks/PP_Rock_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_02.prefab",
            BASE + "/Stones, Rocks/PP_Rock_03.prefab",
            BASE + "/Stones, Rocks/PP_Rock_04.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Pile_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Pile_02.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Moss_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Moss_02.prefab",
            BASE + "/Stones, Rocks/PP_Stalagmite_01.prefab",
            BASE + "/Stones, Rocks/PP_Stalagmite_02.prefab",
            BASE + "/Stones, Rocks/PP_Stalagmite_03.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Menhir_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Column_01.prefab",

            // ── 7. Mağara Duvar Parçaları (Normal 1.0x) ──
            BASE + "/Cave/PP_Cave_Wall_Even_01.prefab",
            BASE + "/Cave/PP_Cave_Wall_Even_Veins_01_Gold.prefab",
            BASE + "/Cave/PP_Cave_Wall_Even_Veins_01_Blue.prefab",
            BASE + "/Cave/PP_Cliff_01.prefab"
        };

        // Üst üste binmeyi önlemek için zemin alanını grid hücrelerine bölelim
        int cols = 8;  // X yönü
        int rows = 10; // Z yönü

        float cellWidth = (GROUND_MAX_X - GROUND_MIN_X) / cols;
        float cellHeight = (GROUND_MAX_Z - GROUND_MIN_Z) / rows;

        System.Random rng = new System.Random(2026);

        // Havuzu karıştır
        Shuffle(assetPool, rng);
        int poolIndex = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Hücre merkezini hesapla
                float cellCenterX = GROUND_MIN_X + (c + 0.5f) * cellWidth;
                float cellCenterZ = GROUND_MIN_Z + (r + 0.5f) * cellHeight;

                // Hücre içinde hafif rastgele ofset (birbirine çarpmayacak kadar)
                float jx = (float)(rng.NextDouble() - 0.5) * (cellWidth * 0.45f);
                float jz = (float)(rng.NextDouble() - 0.5) * (cellHeight * 0.45f);

                Vector3 spawnPos = new Vector3(cellCenterX + jx, 0.0f, cellCenterZ + jz);

                // Sıradaki prefabı seç
                string prefabPath = assetPool[poolIndex % assetPool.Count];
                poolIndex++;

                // Normal, insan gözünün net görebileceği orijinal boyut (0.95 - 1.1x)
                float scale = 0.95f + (float)rng.NextDouble() * 0.15f;
                float rotY = (float)rng.NextDouble() * 360f;

                Spawn(prefabPath, spawnPos, rotY, scale, root);
            }
        }

        Undo.CollapseUndoOperations(group);
        Debug.Log($"<color=lime><b>[AbandonedMineSceneBuilder]</b> {cols * rows} adet maden asseti normal boyutlarinda ve aralikli olarak ground uzerine dagitildi!</color>");
    }

    [MenuItem("Tools/Mine Dungeon Builder/Clear Abandoned Mine Objects")]
    public static void ClearMenu()
    {
        ClearOldMineObjects(true);
    }

    private static void ClearOldMineObjects(bool showDialog)
    {
        var existing = GameObject.Find(ROOT_NAME);
        if (existing != null)
        {
            if (!showDialog || EditorUtility.DisplayDialog("Temizle", ROOT_NAME + " objesi ve altindaki tum assetler silinecek.", "Sil", "Iptal"))
            {
                Undo.DestroyObjectImmediate(existing);
                Debug.Log("<color=yellow>[AbandonedMineSceneBuilder] Onceki maden objeleri temizlendi.</color>");
            }
        }
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private static GameObject Spawn(string assetPath, Vector3 pos, float rotY, float uniformScale, GameObject parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning("[AbandonedMineSceneBuilder] Prefab bulunamadi: " + assetPath);
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(go, "Spawn " + prefab.name);

        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, rotY, 0);
        go.transform.localScale = Vector3.one * uniformScale;
        go.transform.SetParent(parent.transform);

        GameObjectUtility.SetStaticEditorFlags(go,
            StaticEditorFlags.ContributeGI |
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.BatchingStatic);

        return go;
    }
}
#endif

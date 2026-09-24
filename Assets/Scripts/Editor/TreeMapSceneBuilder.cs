#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pictures/TreeMap.png konsept gorselini GameScene uzerinde
/// ground sinirlarina tam oturacak sekilde insa eden Editor araci.
/// 
/// Menu: Tools > TreeMap Builder > Build Complete TreeMap Scene
/// </summary>
public static class TreeMapSceneBuilder
{
    // ── Ground Sinirlari (GameScene olcum degerleri) ──────────────────────
    private const float GROUND_MIN_X = -23.0f;
    private const float GROUND_MAX_X =  92.0f;
    private const float GROUND_MIN_Z = -92.0f;
    private const float GROUND_MAX_Z =  70.0f;

    private const float CENTER_X =  34.5f;
    private const float CENTER_Z = -11.0f;

    // Prefab kok dizini
    private const string BASE = "Assets/New assets/PurePoly/Mining_Pack/Prefabs";
    private const string ROOT_NAME = "[TreeMap_Scene]";

    // ── Menü Butonları ───────────────────────────────────────────────────

    [MenuItem("Tools/TreeMap Builder/Build Complete TreeMap Scene")]
    public static void BuildCompleteScene()
    {
        if (!EditorUtility.DisplayDialog("TreeMap Sahne Kurulumu",
            "Pictures/TreeMap.png gorselindeki harita duzeni GameScene ground sinirlari icinde olusturulacak.\n\n" +
            "Ctrl+Z ile istediginiz an geri alabilirsiniz.\nDevam edilsin mi?",
            "Evet, Olustur", "Iptal"))
            return;

        Undo.SetCurrentGroupName("Build TreeMap Scene");
        int group = Undo.GetCurrentGroup();

        // Varsa eskiyi temizle
        ClearOldTreeMapObjects(false);

        GameObject root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Create TreeMap Root");

        BuildPerimeterBelt(root);
        BuildGiantBoulderHill(root);
        BuildCampsite(root);
        BuildGatesAndBarricades(root);
        BuildArenaObstacles(root);

        Undo.CollapseUndoOperations(group);

        Debug.Log("<color=lime><b>[TreeMapSceneBuilder]</b> TreeMap.png duzeni GameScene uzerinde basariyla olusturuldu! Ctrl+Z ile geri alabilirsiniz.</color>");
    }

    [MenuItem("Tools/TreeMap Builder/Clear TreeMap Objects")]
    public static void ClearMenu()
    {
        ClearOldTreeMapObjects(true);
    }

    private static void ClearOldTreeMapObjects(bool showDialog)
    {
        var existing = GameObject.Find(ROOT_NAME);
        if (existing != null)
        {
            if (!showDialog || EditorUtility.DisplayDialog("Temizle", ROOT_NAME + " objesi ve altindaki tum assetler silinecek.", "Sil", "Iptal"))
            {
                Undo.DestroyObjectImmediate(existing);
                Debug.Log("<color=yellow>[TreeMapSceneBuilder] Onceki TreeMap objeleri temizlendi.</color>");
            }
        }
    }

    // ── 1. Dış Çevre: Ağaç ve Kaya Kuşağı ─────────────────────────────────
    private static void BuildPerimeterBelt(GameObject root)
    {
        GameObject parent = CreateChild(root, "01_Perimeter_Belt");

        string[] firTrees = {
            BASE + "/Vegetation/PP_Fir_Tree_01.prefab",
            BASE + "/Vegetation/PP_Fir_Tree_02.prefab",
            BASE + "/Vegetation/PP_Fir_Tree_03.prefab",
            BASE + "/Vegetation/PP_Fir_Tree_04.prefab",
            BASE + "/Vegetation/PP_Fir_Tree_05.prefab",
        };

        string[] roundTrees = {
            BASE + "/Vegetation/PP_Tree_01.prefab",
            BASE + "/Vegetation/PP_Tree_02.prefab",
            BASE + "/Vegetation/PP_Tree_03.prefab",
            BASE + "/Vegetation/PP_Tree_04.prefab",
            BASE + "/Vegetation/PP_Tree_05.prefab",
        };

        string[] leaflessTrees = {
            BASE + "/Vegetation/PP_Leafless_Tree_01.prefab",
            BASE + "/Vegetation/PP_Leafless_Tree_02.prefab",
            BASE + "/Vegetation/PP_Leafless_Tree_03.prefab",
            BASE + "/Vegetation/PP_Leafless_Tree_04.prefab",
        };

        string[] borderRocks = {
            BASE + "/Stones, Rocks/PP_Rock_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_02.prefab",
            BASE + "/Stones, Rocks/PP_Rock_03.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Moss_01.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Moss_02.prefab",
            BASE + "/Stones, Rocks/PP_Rock_Pile_01.prefab",
        };

        System.Random rng = new System.Random(1337);

        // Sinir cizgisini belirle (Ground'dan 3 birim iceride)
        float minX = GROUND_MIN_X + 3.0f;
        float maxX = GROUND_MAX_X - 3.0f;
        float minZ = GROUND_MIN_Z + 3.0f;
        float maxZ = GROUND_MAX_Z - 3.0f;

        float step = 4.2f; // agac sikligi

        List<Vector3> perimeterSlots = new List<Vector3>();

        // Kuzey kenar (Z max)
        for (float x = minX; x <= maxX; x += step)
            perimeterSlots.Add(new Vector3(x, 0, maxZ));

        // Dogu kenar (X max)
        for (float z = maxZ - step; z >= minZ; z -= step)
            perimeterSlots.Add(new Vector3(maxX, 0, z));

        // Guney kenar (Z min)
        for (float x = maxX - step; x >= minX; x -= step)
            perimeterSlots.Add(new Vector3(x, 0, minZ));

        // Bati kenar (X min)
        for (float z = minZ + step; z < maxZ; z += step)
            perimeterSlots.Add(new Vector3(minX, 0, z));

        foreach (var basePos in perimeterSlots)
        {
            // Guney ana kapi bolgesini (giris yolunu) acik birak
            if (basePos.z < minZ + 8f && Mathf.Abs(basePos.x - 30f) < 9f)
                continue;

            // Sag ust tepe bolgesini biraz esnet
            if (basePos.x > 62f && basePos.z > 32f)
                continue;

            // 1. Ana Çam Ağacı (Dış çizgi)
            float jx = (float)(rng.NextDouble() - 0.5) * 1.6f;
            float jz = (float)(rng.NextDouble() - 0.5) * 1.6f;
            Vector3 treePos = new Vector3(basePos.x + jx, 0, basePos.z + jz);

            string firPrefab = firTrees[rng.Next(firTrees.Length)];
            float scale = 0.9f + (float)rng.NextDouble() * 0.45f;
            Spawn(firPrefab, treePos, (float)rng.NextDouble() * 360f, scale, parent);

            // 2. Yanina rastgele yuvarlak turuncu agac / kuru agac / kaya serp
            double roll = rng.NextDouble();
            Vector3 innerOffset = (new Vector3(CENTER_X, 0, CENTER_Z) - basePos).normalized * (2.0f + (float)rng.NextDouble() * 2.0f);
            Vector3 secPos = basePos + innerOffset + new Vector3(jx, 0, jz);

            if (roll < 0.35)
            {
                string rTree = roundTrees[rng.Next(roundTrees.Length)];
                Spawn(rTree, secPos, (float)rng.NextDouble() * 360f, 0.75f + (float)rng.NextDouble() * 0.35f, parent);
            }
            else if (roll < 0.55)
            {
                string lTree = leaflessTrees[rng.Next(leaflessTrees.Length)];
                Spawn(lTree, secPos, (float)rng.NextDouble() * 360f, 0.85f + (float)rng.NextDouble() * 0.3f, parent);
            }
            else if (roll < 0.85)
            {
                string rock = borderRocks[rng.Next(borderRocks.Length)];
                Spawn(rock, secPos, (float)rng.NextDouble() * 360f, 0.7f + (float)rng.NextDouble() * 0.5f, parent);
            }
        }
    }

    // ── 2. Sağ Üst Köşe: Dev Kaya ve Tepe (The Great Boulder Hill) ─────────
    private static void BuildGiantBoulderHill(GameObject root)
    {
        GameObject parent = CreateChild(root, "02_Giant_Boulder_Hill");

        Vector3 hillCenter = new Vector3(70.0f, 0f, 40.0f);

        // Taban tepesi / plato
        Spawn(BASE + "/Environment/PP_Rock_Plateau_01.prefab", hillCenter + new Vector3(0, -0.3f, 0), 45f, 2.8f, parent);
        Spawn(BASE + "/Environment/PP_Rock_Plateau_02.prefab", hillCenter + new Vector3(2f, 0.5f, -2f), 120f, 2.2f, parent);

        // Görseldeki DEV KÜRESEL KAYA (Merkez anıt kaya)
        Vector3 boulderPos = hillCenter + new Vector3(0f, 2.5f, 0f);
        GameObject giantBoulder = Spawn(BASE + "/Stones, Rocks/PP_Rock_01.prefab", boulderPos, 25f, 10.5f, parent);
        if (giantBoulder != null)
        {
            giantBoulder.transform.localScale = new Vector3(11.0f, 9.5f, 10.5f);
        }

        // Tepe etrafındaki destek kayaları
        Spawn(BASE + "/Stones, Rocks/PP_Rock_03.prefab", hillCenter + new Vector3(-8f, 0, -4f), 70f, 2.5f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_02.prefab", hillCenter + new Vector3(7f, 0, -5f), 190f, 2.2f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_01.prefab", hillCenter + new Vector3(-5f, 0, -8f), 30f, 1.8f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Moss_02.prefab", hillCenter + new Vector3(-2f, 0, -9f), 140f, 1.6f, parent);

        // Tepe eteğindeki küçük çamlar ve yuvarlak ağaçlar
        Spawn(BASE + "/Vegetation/PP_Fir_Tree_01.prefab", hillCenter + new Vector3(-10f, 0, -8f), 10f, 0.8f, parent);
        Spawn(BASE + "/Vegetation/PP_Fir_Tree_03.prefab", hillCenter + new Vector3(10f, 0, -3f), 80f, 0.85f, parent);
        Spawn(BASE + "/Vegetation/PP_Tree_02.prefab", hillCenter + new Vector3(-6f, 0, -11f), 45f, 0.75f, parent);
        Spawn(BASE + "/Vegetation/PP_Tree_04.prefab", hillCenter + new Vector3(5f, 0, -9f), 160f, 0.7f, parent);
    }

    // ── 3. Sağ Taraf: Kamp Alanı (The Campsite) ───────────────────────────
    private static void BuildCampsite(GameObject root)
    {
        GameObject parent = CreateChild(root, "03_Campsite");

        Vector3 campCenter = new Vector3(72.0f, 0f, -22.0f);

        // Çadır
        Spawn(BASE + "/Props/PP_Tent_01.prefab", campCenter, -35.0f, 1.4f, parent);

        // Çadır önü kamp ateşi
        Spawn(BASE + "/Props/PP_Campfire_01.prefab", campCenter + new Vector3(-6.5f, 0f, -6.0f), 0f, 1.2f, parent);

        // Kamp etrafındaki odunlar ve fıçılar / sandıklar
        Spawn(BASE + "/Props/PP_Log_Pile_01.prefab", campCenter + new Vector3(5.5f, 0f, 2.5f), 15f, 1.2f, parent);
        Spawn(BASE + "/Props/PP_Crate_Wooden_01.prefab", campCenter + new Vector3(4.0f, 0f, -3.0f), 40f, 1.0f, parent);
        Spawn(BASE + "/Props/PP_Barrel_01.prefab", campCenter + new Vector3(3.0f, 0f, -4.5f), 10f, 1.0f, parent);
        Spawn(BASE + "/Props/PP_Treasure_Chest_01_Gold.prefab", campCenter + new Vector3(5.0f, 0f, -1.0f), 65f, 0.9f, parent);

        // Çadır arkası ağaçlar
        Spawn(BASE + "/Vegetation/PP_Tree_01.prefab", campCenter + new Vector3(7.0f, 0f, -8.0f), 20f, 0.9f, parent);
        Spawn(BASE + "/Vegetation/PP_Tree_03.prefab", campCenter + new Vector3(8.0f, 0f, 5.0f), 130f, 0.85f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_04.prefab", campCenter + new Vector3(9.0f, 0f, -1.0f), 0f, 1.5f, parent);
    }

    // ── 4. Barikatlar ve Kapılar (Gates & Barricades) ──────────────────────
    private static void BuildGatesAndBarricades(GameObject root)
    {
        GameObject parent = CreateChild(root, "04_Barricades_Gates");

        // A) GÜNEY GİRİŞ KAPISI (Main South Entrance)
        Vector3 southGate = new Vector3(29.0f, 0f, -74.0f);

        // Girişteki tahta köprü / zemin
        Spawn(BASE + "/Blacksmith House/PP_Wooden_Floor_01.prefab", southGate + new Vector3(0, 0.05f, 0), 0f, 1.5f, parent);

        // Girişin sol kanat sivri kazıklı barikatı
        Spawn(BASE + "/Props/PP_Log_Fence_01.prefab", southGate + new Vector3(-6.0f, 0f, 2.0f), 30f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_02.prefab", southGate + new Vector3(-9.0f, 0f, 0f), 15f, 1.5f, parent);

        // Girişin sağ kanat sivri kazıklı barikatı
        Spawn(BASE + "/Props/PP_Log_Fence_01.prefab", southGate + new Vector3(6.5f, 0f, 2.0f), -30f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_03.prefab", southGate + new Vector3(9.5f, 0f, 0f), 75f, 1.4f, parent);

        // B) KUZEY BARİKATI (North Barricade)
        Vector3 northGate = new Vector3(31.0f, 0f, 48.0f);
        Spawn(BASE + "/Props/PP_Log_Fence_01.prefab", northGate + new Vector3(-3.5f, 0f, 0f), -15f, 1.25f, parent);
        Spawn(BASE + "/Props/PP_Log_Fence_02.prefab", northGate + new Vector3(3.5f, 0f, 0f), 15f, 1.25f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_01.prefab", northGate + new Vector3(-8.0f, 0f, 1.0f), 45f, 1.8f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_03.prefab", northGate + new Vector3(8.0f, 0f, -1.0f), 120f, 1.4f, parent);

        // C) DOĞU KAMP ÖNÜ BARİKATI (East Campsite Barricade)
        Vector3 eastBarricade = new Vector3(58.0f, 0f, -10.0f);
        Spawn(BASE + "/Props/PP_Log_Fence_01.prefab", eastBarricade + new Vector3(0f, 0f, -4f), 75f, 1.3f, parent);
        Spawn(BASE + "/Props/PP_Log_Fence_02.prefab", eastBarricade + new Vector3(0f, 0f, 4f), 85f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_02.prefab", eastBarricade + new Vector3(-1f, 0f, 9f), 0f, 1.5f, parent);
    }

    // ── 5. İç Arena Engelleri (Arena Obstacles & Standing Stones) ─────────
    private static void BuildArenaObstacles(GameObject root)
    {
        GameObject parent = CreateChild(root, "05_Arena_Obstacles");

        // A) SOL KANAT DİKİLİ TAŞLAR VE MENHİRLER (Standing Stones)
        Vector3 leftStone1 = new Vector3(12.0f, 0f, 18.0f);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_01.prefab", leftStone1, 20f, 1.6f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_03.prefab", leftStone1 + new Vector3(2.5f, 0f, -2f), 45f, 1.1f, parent);

        Vector3 leftStone2 = new Vector3(16.0f, 0f, 5.0f);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Column_01.prefab", leftStone2, -35f, 1.5f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_02.prefab", leftStone2 + new Vector3(-2f, 0f, -1.5f), 110f, 1.2f, parent);

        Vector3 leftStone3 = new Vector3(10.0f, 0f, -12.0f);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_02.prefab", leftStone3, 85f, 1.7f, parent);

        // B) SAĞ İÇ KAYA GRUBU (Right Inner Rocks)
        Vector3 rightInner = new Vector3(50.0f, 0f, 14.0f);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_03.prefab", rightInner, -60f, 1.5f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Moss_01.prefab", rightInner + new Vector3(2f, 0f, -2.5f), 30f, 1.2f, parent);

        // C) ORTA-GÜNEY KAYA GRUBU (Mid-South Rocks)
        Vector3 midSouth = new Vector3(36.0f, 0f, -34.0f);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_02.prefab", midSouth, 15f, 1.5f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_01.prefab", midSouth + new Vector3(-3f, 0f, 1.5f), 160f, 1.0f, parent);

        // D) ARENA İÇİ KURU AĞAÇLAR (Dead Trees in Clearing)
        Spawn(BASE + "/Vegetation/PP_Leafless_Tree_01.prefab", new Vector3(18.0f, 0f, 26.0f), 40f, 1.1f, parent);
        Spawn(BASE + "/Vegetation/PP_Leafless_Tree_02.prefab", new Vector3(14.0f, 0f, -26.0f), -80f, 1.0f, parent);
        Spawn(BASE + "/Vegetation/PP_Leafless_Tree_03.prefab", new Vector3(45.0f, 0f, -42.0f), 125f, 1.0f, parent);

        // E) GÖRSELDEKİ KÜÇÜK SERPME KAYALAR (Scatter Rocks)
        Spawn(BASE + "/Stones, Rocks/PP_Rock_05.prefab", new Vector3(25.0f, 0f, 10.0f), 15f, 0.7f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_06.prefab", new Vector3(42.0f, 0f, 5.0f), 70f, 0.8f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_07.prefab", new Vector3(28.0f, 0f, -18.0f), -40f, 0.75f, parent);
    }

    // ── Yardımcı Araçlar ──────────────────────────────────────────────────
    private static GameObject CreateChild(GameObject root, string name)
    {
        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create Subgroup " + name);
        child.transform.SetParent(root.transform);
        return child;
    }

    private static GameObject Spawn(string assetPath, Vector3 pos, float rotY, float uniformScale, GameObject parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning("[TreeMapSceneBuilder] Prefab bulunamadi: " + assetPath);
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(go, "Spawn " + prefab.name);

        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, rotY, 0);
        go.transform.localScale = Vector3.one * uniformScale;
        go.transform.SetParent(parent.transform);

        // Static bayraklari
        GameObjectUtility.SetStaticEditorFlags(go,
            StaticEditorFlags.ContributeGI |
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.BatchingStatic);

        return go;
    }
}
#endif

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// GameScene icin asset yerlestirici.
/// Menu: Tools > Scene Setup > Place GameScene Assets
/// Ground siniri: X[-23, 92]  Z[-92, 70]  Merkez(34.4, -11.4)
/// </summary>
public static class GameScenePlacer
{
    // ── Ground sinirlari (sahneden olculen degerler) ──────────────────────
    private const float GROUND_MIN_X = -23f;
    private const float GROUND_MAX_X =  92f;
    private const float GROUND_MIN_Z = -92f;
    private const float GROUND_MAX_Z =  70f;
    private const float CENTER_X     =  34.4f;
    private const float CENTER_Z     = -11.4f;

    // Merkez saldiri alani bos birakilsin (boss spawn + oyuncu hareketi)
    private const float CLEAR_RADIUS  = 18f;   // merkez etrafinda bos alan
    // Kenar marji: ground disina tasmamak icin ic tarafa cek
    private const float BORDER_MARGIN =  4f;

    // Yerlestirilecek prefab yollari (PurePoly Mining Pack)
    private const string BASE_PATH = "Assets/New assets/PurePoly/Mining_Pack/Prefabs";

    // ── Menü girişleri ────────────────────────────────────────────────────

    [MenuItem("Tools/Scene Setup/Place GameScene Assets (FULL)")]
    public static void PlaceAll()
    {
        if (!EditorUtility.DisplayDialog("GameScene Asset Yerlesimi",
            "Bu islem sahneye ~80 yeni obje ekleyecek.\nGeri almak icin Ctrl+Z kullanabilirsin.\n\nDevam et?",
            "Evet, Yerlesir", "Iptal"))
            return;

        Undo.SetCurrentGroupName("GameScene Asset Placement");
        int group = Undo.GetCurrentGroup();

        PlaceBorderTrees();
        PlaceBorderRocks();
        PlaceInnerProps();
        PlacePillarsArenaEdge();

        Undo.CollapseUndoOperations(group);
        Debug.Log("<color=lime>[GameScenePlacer] Tum assetler yerlestirildi! Ctrl+Z ile geri alabilirsin.</color>");
    }

    [MenuItem("Tools/Scene Setup/Place Border Trees Only")]
    public static void PlaceBorderTreesMenu() { PlaceBorderTrees(); }

    [MenuItem("Tools/Scene Setup/Place Border Rocks Only")]
    public static void PlaceBorderRocksMenu() { PlaceBorderRocks(); }

    [MenuItem("Tools/Scene Setup/Place Inner Props Only")]
    public static void PlaceInnerPropsMenu() { PlaceInnerProps(); }

    [MenuItem("Tools/Scene Setup/Place Arena Pillars Only")]
    public static void PlacePillarsMenu() { PlacePillarsArenaEdge(); }

    [MenuItem("Tools/Scene Setup/CLEAR All Placed Assets")]
    public static void ClearAll()
    {
        if (!EditorUtility.DisplayDialog("Temizle",
            "GameScenePlacer tarafindan etiketlenen tum objeler silinecek.", "Sil", "Iptal"))
            return;

        var objs = GameObject.FindGameObjectsWithTag("EditorPlaced");
        foreach (var o in objs) Undo.DestroyObjectImmediate(o);
        Debug.Log($"<color=yellow>[GameScenePlacer] {objs.Length} obje silindi.</color>");
    }

    // ── Ağaç perdesi (sınır boyunca) ─────────────────────────────────────
    private static void PlaceBorderTrees()
    {
        string[] treePrefabs = {
            BASE_PATH + "/Vegetation/PP_Fir_Tree_01.prefab",
            BASE_PATH + "/Vegetation/PP_Fir_Tree_03.prefab",
            BASE_PATH + "/Vegetation/PP_Fir_Tree_05.prefab",
            BASE_PATH + "/Vegetation/PP_Fantasy_Birch_Tree_01.prefab",
            BASE_PATH + "/Vegetation/PP_Fantasy_Birch_Tree_03.prefab",
            BASE_PATH + "/Vegetation/PP_Leafless_Tree_02.prefab",
        };

        var parent = GetOrCreateParent("== Border Trees ==");
        var rng    = new System.Random(42);

        // Kenar noktalarini olustur: 4 kenarda es aralikli
        float margin = BORDER_MARGIN + 1f;
        float spacing = 8f;
        var points = new List<Vector3>();

        // Kuzey kenar (Z max)
        for (float x = GROUND_MIN_X + margin; x <= GROUND_MAX_X - margin; x += spacing)
            points.Add(new Vector3(x, 0, GROUND_MAX_Z - margin));

        // Guney kenar (Z min)
        for (float x = GROUND_MIN_X + margin; x <= GROUND_MAX_X - margin; x += spacing)
            points.Add(new Vector3(x, 0, GROUND_MIN_Z + margin));

        // Bati kenar (X min)
        for (float z = GROUND_MIN_Z + margin + spacing; z <= GROUND_MAX_Z - margin - spacing; z += spacing)
            points.Add(new Vector3(GROUND_MIN_X + margin, 0, z));

        // Dogu kenar (X max)
        for (float z = GROUND_MIN_Z + margin + spacing; z <= GROUND_MAX_Z - margin - spacing; z += spacing)
            points.Add(new Vector3(GROUND_MAX_X - margin, 0, z));

        foreach (var p in points)
        {
            // Merkez saldiri alanini temiz tut
            if (Vector2.Distance(new Vector2(p.x, p.z), new Vector2(CENTER_X, CENTER_Z)) < CLEAR_RADIUS + 4f)
                continue;

            // Hafif rastgele ofset
            float ox = (float)(rng.NextDouble() - 0.5) * 2.5f;
            float oz = (float)(rng.NextDouble() - 0.5) * 2.5f;
            var pos = new Vector3(p.x + ox, 0, p.z + oz);

            // Ground icinde mi?
            if (!IsInsideGround(pos, 1f)) continue;

            string prefabPath = treePrefabs[rng.Next(treePrefabs.Length)];
            float  rotY       = (float)(rng.NextDouble() * 360.0);
            float  scale      = 0.8f + (float)(rng.NextDouble() * 0.6f);  // 0.8 - 1.4

            SpawnPrefab(prefabPath, pos, rotY, scale, parent);
        }
        Debug.Log($"[GameScenePlacer] Agac perdesi yerlestirildi ({points.Count} nokta islendi).");
    }

    // ── Kaya kümesi (orta-kenar bölgesi) ─────────────────────────────────
    private static void PlaceBorderRocks()
    {
        string[] rockPrefabs = {
            BASE_PATH + "/Stones, Rocks/PP_Rock_01.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_03.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_05.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Pile_01.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Pile_04.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Moss_01.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Moss_03.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Column_01.prefab",
            BASE_PATH + "/Stones, Rocks/PP_Rock_Menhir_01.prefab",
        };

        var parent = GetOrCreateParent("== Border Rocks ==");
        var rng    = new System.Random(99);
        int count  = 0;

        // Kenar bolgede rastgele kaya kume
        for (int i = 0; i < 55; i++)
        {
            float x = Lerp(GROUND_MIN_X, GROUND_MAX_X, (float)rng.NextDouble(), rng);
            float z = Lerp(GROUND_MIN_Z, GROUND_MAX_Z, (float)rng.NextDouble(), rng);
            var pos = new Vector3(x, 0, z);

            float distCenter = Vector2.Distance(new Vector2(x, z), new Vector2(CENTER_X, CENTER_Z));
            // Merkezi bos birak; cok ic ya da cok dis olmasin
            if (distCenter < CLEAR_RADIUS) continue;
            if (!IsInsideGround(pos, 2.5f)) continue;

            string prefabPath = rockPrefabs[rng.Next(rockPrefabs.Length)];
            float  rotY       = (float)(rng.NextDouble() * 360.0);
            float  scale      = 0.6f + (float)(rng.NextDouble() * 1.0f);

            SpawnPrefab(prefabPath, pos, rotY, scale, parent);
            count++;
        }
        Debug.Log($"[GameScenePlacer] {count} kaya kume yerlestirildi.");
    }

    // ── İç prop'lar (kuyu, ocak, varil) ──────────────────────────────────
    private static void PlaceInnerProps()
    {
        var parent = GetOrCreateParent("== Inner Props ==");
        var rng    = new System.Random(7);

        // Sabit, akla yatkin prop noktalari
        var propDefs = new (string path, Vector3 pos, float rotY, float scale)[]
        {
            // Kuzeybati kösesinde demirci grubu
            (BASE_PATH + "/Props/PP_Furnace_01.prefab",      new Vector3(-5f, 0, 50f),   0f,   1f),
            (BASE_PATH + "/Props/PP_Barrel_01.prefab",       new Vector3(-8f, 0, 48f),  45f,   1f),
            (BASE_PATH + "/Props/PP_Barrel_03.prefab",       new Vector3(-6f, 0, 46f), 120f,   1f),
            (BASE_PATH + "/Props/PP_Wooden_Table_01.prefab", new Vector3(-3f, 0, 50f),  90f,   1f),
            (BASE_PATH + "/Props/PP_Log_Pile_01.prefab",     new Vector3(-2f, 0, 47f),   0f,   1f),
            (BASE_PATH + "/Props/PP_Crate_Wooden_01.prefab", new Vector3(-9f, 0, 51f),  30f,   1f),

            // Dogu tarafinda kuyu + sandik
            (BASE_PATH + "/Props/PP_Well_01.prefab",                    new Vector3(78f, 0, -5f),  0f, 1f),
            (BASE_PATH + "/Props/PP_Treasure_Chest_01_Gold.prefab",     new Vector3(80f, 0, -9f), 45f, 1f),
            (BASE_PATH + "/Props/PP_Campfire_01.prefab",                new Vector3(75f, 0, -2f),  0f, 1f),

            // Guney bolgesi - cader + sandik
            (BASE_PATH + "/Props/PP_Tent_01.prefab",          new Vector3(20f, 0, -78f),  90f, 1f),
            (BASE_PATH + "/Props/PP_Tent_03.prefab",          new Vector3(30f, 0, -80f), 180f, 1f),
            (BASE_PATH + "/Props/PP_Log_Pile_03.prefab",      new Vector3(25f, 0, -75f),   0f, 1f),
            (BASE_PATH + "/Props/PP_Campfire_02.prefab",      new Vector3(22f, 0, -72f),   0f, 1f),

            // Kuzey merkez civari
            (BASE_PATH + "/Props/PP_Signpost_01.prefab",      new Vector3(34f, 0,  58f),   0f, 1f),
        };

        foreach (var def in propDefs)
        {
            var pos = def.pos;
            if (!IsInsideGround(pos, 2f)) continue;
            float dist = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(CENTER_X, CENTER_Z));
            if (dist < CLEAR_RADIUS) continue;

            SpawnPrefab(def.path, pos, def.rotY, def.scale, parent);
        }
        Debug.Log("[GameScenePlacer] Inner props yerlestirildi.");
    }

    // ── Arena kenar sütunları ─────────────────────────────────────────────
    private static void PlacePillarsArenaEdge()
    {
        string pillarPath = BASE_PATH + "/Pillars, Runes/PP_Pillar_Stone_01.prefab";
        string runePath   = BASE_PATH + "/Pillars, Runes/PP_Rune_Stone_01.prefab";

        var parent = GetOrCreateParent("== Arena Pillars ==");

        // 4 ana sutun - arena cercevesi (CLEAR_RADIUS kenarinda)
        float r = CLEAR_RADIUS - 1f;
        float[] angles = { 45f, 135f, 225f, 315f };
        foreach (float a in angles)
        {
            float rad = a * Mathf.Deg2Rad;
            var pos   = new Vector3(CENTER_X + Mathf.Cos(rad) * r, 0, CENTER_Z + Mathf.Sin(rad) * r);
            if (IsInsideGround(pos, 1f))
                SpawnPrefab(pillarPath, pos, a, 1.2f, parent);
        }

        // 4 run tasi (suttunlar arasi)
        float[] runeAngles = { 0f, 90f, 180f, 270f };
        foreach (float a in runeAngles)
        {
            float rad = a * Mathf.Deg2Rad;
            var pos   = new Vector3(CENTER_X + Mathf.Cos(rad) * (r + 2f), 0, CENTER_Z + Mathf.Sin(rad) * (r + 2f));
            if (IsInsideGround(pos, 1f))
                SpawnPrefab(runePath, pos, a + 45f, 1f, parent);
        }
        Debug.Log("[GameScenePlacer] Arena sutunlari yerlestirildi.");
    }

    // ── Yardımcı metodlar ─────────────────────────────────────────────────
    private static bool IsInsideGround(Vector3 pos, float margin)
    {
        return pos.x >= GROUND_MIN_X + margin && pos.x <= GROUND_MAX_X - margin &&
               pos.z >= GROUND_MIN_Z + margin && pos.z <= GROUND_MAX_Z - margin;
    }

    private static float Lerp(float min, float max, float t, System.Random rng)
    {
        return min + (max - min) * (float)rng.NextDouble();
    }

    private static GameObject GetOrCreateParent(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create Parent " + name);
        return go;
    }

    private static void SpawnPrefab(string assetPath, Vector3 pos, float rotY, float scale, GameObject parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[GameScenePlacer] Prefab bulunamadi: {assetPath}");
            return;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(go, "Place " + prefab.name);

        go.transform.position   = pos;
        go.transform.rotation   = Quaternion.Euler(0, rotY, 0);
        go.transform.localScale = Vector3.one * scale;
        go.transform.SetParent(parent.transform);

        // Etiketle ki temizlemek kolaylassin (Unity'de "EditorPlaced" tag olusturulmalidir)
        // go.tag = "EditorPlaced";  // Tag yoksa hata verir, yorum satiri

        // Static yap (performans icin)
        GameObjectUtility.SetStaticEditorFlags(go,
            StaticEditorFlags.ContributeGI |
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.BatchingStatic);
    }
}
#endif

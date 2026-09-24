using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ===========================================================================
//  NewMap3Scene – Referans resme göre harita düzeni
//  NewMap3picture.png referans alınarak oluşturulmuştur:
//  - Tüm kenar: büyük gri yosunlu kayalar (perimeter)
//  - Sol-üst: gölet (PP_Lake)
//  - Orta: ahşap köprü platformları (PP_Wooden_Floor)
//  - Sağ: ahşap barikat/çitler (PP_Wooden_Fence, PP_Log_Wall)
//  - İçeride: dağınık kaya grupları
//  - Perimeter kayaların yanı: tırmanma bitkileri
//  - Sol giriş: ahşap kemer (PP_Mine_Entrance_Wooden)
// ===========================================================================
public class NewMap3LayoutGenerator : EditorWindow
{
    [MenuItem("Tools/Generate NewMap3 Picture Layout")]
    public static void GenerateMap()
    {
        if (SceneManager.GetActiveScene().name != "NewMap3Scene")
        {
            if (EditorUtility.DisplayDialog("Yanlis Sahne",
                "'NewMap3Scene' sahnesinde calistirmalisiniz. Acilsin mi?", "Evet", "Hayir"))
                EditorSceneManager.OpenScene("Assets/Scenes/NewMap3Scene.unity");
            else return;
        }

        // Onceki nesilleri temizle
        foreach (string n in new[] { "[Crystal_Mine_Arena]", "[NewMap3_Layout]" })
        {
            var old = GameObject.Find(n);
            if (old != null) DestroyImmediate(old);
        }

        // Ground merkezini bul
        var groundObj = GameObject.Find("Ground");
        Vector3 C = new Vector3(-1.18f, 0.22f, 3.21f);
        if (groundObj != null)
        {
            var bc = groundObj.GetComponent<BoxCollider>();
            if (bc != null)
            {
                C = groundObj.transform.TransformPoint(bc.center);
                C.y = groundObj.transform.position.y + 0.22f;
            }
        }
        Debug.Log("[NewMap3Layout] Arena merkezi: " + C);

        // Hiyerarsi
        var root       = new GameObject("[NewMap3_Layout]");
        var gPerimeter = Child(root, "01_Perimeter_Rocks");
        var gLake      = Child(root, "02_Lake");
        var gBridge    = Child(root, "03_Bridge_Platforms");
        var gBarricade = Child(root, "04_Barricades");
        var gInnerRock = Child(root, "05_Inner_Rocks");
        var gVeg       = Child(root, "06_Vegetation");
        var gEntrance  = Child(root, "07_Entrance");

        string B  = "Assets/New assets/PurePoly/Mining_Pack/Prefabs/";
        string SR = B + "Stones, Rocks/";
        string BH = B + "Blacksmith House/";
        string EV = B + "Environment/";
        string VG = B + "Vegetation/";
        string CV = B + "Cave/";

        // ═══════════════════════════════════════════════════════════════════
        // 1. PERIMETER KAYALARI – kenar boyunca yosunlu gri kayalar
        //    Scale 0.35–0.43, her kenar icin ayri nokta listesi
        // ═══════════════════════════════════════════════════════════════════
        string[] rockPool =
        {
            SR + "PP_Rock_Moss_01.prefab",
            SR + "PP_Rock_Moss_02.prefab",
            SR + "PP_Rock_Moss_03.prefab",
            SR + "PP_Rock_Moss_04.prefab",
            SR + "PP_Rock_Moss_05.prefab",
            SR + "PP_Rock_Pile_Moss_01.prefab",
            SR + "PP_Rock_Pile_Moss_02.prefab",
            SR + "PP_Rock_Pile_Moss_03.prefab",
            SR + "PP_Rock_01.prefab",
            SR + "PP_Rock_02.prefab",
            SR + "PP_Rock_03.prefab",
        };

        // Sol kenar (X=-10.8), Z: -12 -> +12, 11 nokta
        SpawnList(rockPool, new (float, float, float)[]
        {
            (-10.8f, -12.0f, 0.40f), (-10.8f, -10.0f, 0.35f), (-10.8f, -7.5f, 0.42f),
            (-10.8f,  -5.0f, 0.38f), (-10.8f,  -2.5f, 0.40f), (-10.8f,  0.0f, 0.36f),
            (-10.8f,   2.5f, 0.43f), (-10.8f,   5.0f, 0.38f), (-10.8f,  7.5f, 0.41f),
            (-10.8f,  10.0f, 0.37f), (-10.8f,  12.0f, 0.40f),
        }, gPerimeter.transform, C);

        // Sag kenar (X=+10.8), 11 nokta
        SpawnList(rockPool, new (float, float, float)[]
        {
            (10.8f, -12.0f, 0.38f), (10.8f, -10.0f, 0.42f), (10.8f, -7.5f, 0.36f),
            (10.8f,  -5.0f, 0.40f), (10.8f,  -2.5f, 0.43f), (10.8f,  0.0f, 0.37f),
            (10.8f,   2.5f, 0.41f), (10.8f,   5.0f, 0.39f), (10.8f,  7.5f, 0.42f),
            (10.8f,  10.0f, 0.36f), (10.8f,  12.0f, 0.40f),
        }, gPerimeter.transform, C);

        // Ust kenar (Z=-12.5), 8 nokta
        SpawnList(rockPool, new (float, float, float)[]
        {
            (-9.0f, -12.5f, 0.40f), (-6.5f, -12.5f, 0.37f), (-4.0f, -12.5f, 0.42f),
            (-1.5f, -12.5f, 0.38f), ( 1.0f, -12.5f, 0.41f), ( 3.5f, -12.5f, 0.36f),
            ( 6.0f, -12.5f, 0.43f), ( 8.5f, -12.5f, 0.39f),
        }, gPerimeter.transform, C);

        // Alt kenar (Z=+12.5), 8 nokta
        SpawnList(rockPool, new (float, float, float)[]
        {
            (-9.0f, 12.5f, 0.42f), (-6.5f, 12.5f, 0.38f), (-4.0f, 12.5f, 0.41f),
            (-1.5f, 12.5f, 0.36f), ( 1.0f, 12.5f, 0.40f), ( 3.5f, 12.5f, 0.43f),
            ( 6.0f, 12.5f, 0.37f), ( 8.5f, 12.5f, 0.41f),
        }, gPerimeter.transform, C);

        // Kose dolgu kayalari (ikinci sira)
        SpawnList(rockPool, new (float, float, float)[]
        {
            (-9.5f, -11.0f, 0.35f), (9.5f, -11.0f, 0.35f),
            (-9.5f,  11.0f, 0.35f), (9.5f,  11.0f, 0.35f),
        }, gPerimeter.transform, C);

        // ═══════════════════════════════════════════════════════════════════
        // 2. GOLET – sol-ust ceyrek
        //    PP_Lake_01 + PP_Cave_Lake_01 + etrafinda kucuk kayalar
        // ═══════════════════════════════════════════════════════════════════
        Spawn(EV + "PP_Lake_01.prefab",      W(C, -6.5f, -6.0f), Q(0),  gLake.transform, 0.45f);
        Spawn(EV + "PP_Cave_Lake_01.prefab", W(C, -5.0f, -4.5f), Q(45), gLake.transform, 0.35f);

        SpawnList(new[] { SR + "PP_Rock_Moss_01.prefab", SR + "PP_Rock_Moss_02.prefab", SR + "PP_Rock_02.prefab" },
                  new (float, float, float)[]
        {
            (-8.5f, -7.5f, 0.30f), (-7.5f, -8.0f, 0.28f), (-8.0f, -5.0f, 0.32f),
            (-5.5f, -8.5f, 0.29f), (-4.0f, -7.0f, 0.27f),
        }, gLake.transform, C);

        // ═══════════════════════════════════════════════════════════════════
        // 3. AHSAP KOPRU PLATFORMLARI
        //    Ana kol: sol-(-6.5) → sag-(+3.5), yatay
        //    Dikey kol: koprunun ortasindan kuzeye
        //    Ikinci kol: sag-ust koseden sag-alta
        //    Scale: 0.50
        // ═══════════════════════════════════════════════════════════════════
        string floorBig   = BH + "PP_Wooden_Floor_01.prefab";
        string floorSmall = BH + "PP_Wooden_Floor_Small_01.prefab";

        // Ana yatay kol
        foreach (var (ox, oz, rot) in new (float, float, float)[]
            { (-6.5f,-2.5f,0f), (-4.5f,-2.5f,0f), (-2.5f,-2.0f,5f), (-0.5f,-1.5f,0f), (1.5f,-1.0f,0f), (3.5f,-0.5f,0f) })
            Spawn(floorBig, W(C, ox, oz), Q(rot), gBridge.transform, 0.50f);

        // Dikey kol (kuzey)
        foreach (var (ox, oz, rot) in new (float, float, float)[]
            { (-2.0f,-4.5f,90f), (-2.0f,-6.5f,90f) })
            Spawn(floorBig, W(C, ox, oz), Q(rot), gBridge.transform, 0.50f);

        // Sag-ust kolu
        foreach (var (ox, oz, rot) in new (float, float, float)[]
            { (5.0f,1.5f,30f), (6.5f,3.0f,30f), (7.5f,4.5f,60f) })
            Spawn(floorSmall, W(C, ox, oz), Q(rot), gBridge.transform, 0.50f);

        // Alt kol (sag-alt)
        foreach (var (ox, oz, rot) in new (float, float, float)[]
            { (4.5f,5.0f,15f), (5.0f,7.0f,15f), (5.5f,9.0f,0f) })
            Spawn(floorSmall, W(C, ox, oz), Q(rot), gBridge.transform, 0.50f);

        // ═══════════════════════════════════════════════════════════════════
        // 4. AHSAP BARIKATLAR – sag taraf, uc kume halinde
        //    PP_Wooden_Fence, PP_Wooden_Beam_Wall, PP_Log_Wall
        //    Scale: 0.42
        // ═══════════════════════════════════════════════════════════════════
        string fn1 = BH + "PP_Wooden_Fence_01.prefab";
        string fn2 = BH + "PP_Wooden_Fence_02.prefab";
        string bw1 = BH + "PP_Wooden_Beam_Wall_01.prefab";
        string bw2 = BH + "PP_Wooden_Beam_Wall_02.prefab";
        string lw  = BH + "PP_Log_Wall_01.prefab";

        // Sag-ust kume
        foreach (var (ox, oz, rot, path) in new (float, float, float, string)[]
            { (7.5f,-4.5f,0f,bw1), (8.5f,-3.0f,90f,fn1), (8.0f,-1.5f,0f,bw2), (9.0f,-4.0f,45f,lw) })
            Spawn(path, W(C, ox, oz), Q(rot), gBarricade.transform, 0.42f);

        // Sag-orta kume
        foreach (var (ox, oz, rot, path) in new (float, float, float, string)[]
            { (8.5f,2.5f,0f,fn2), (8.0f,4.0f,90f,bw1), (9.0f,5.5f,45f,fn1) })
            Spawn(path, W(C, ox, oz), Q(rot), gBarricade.transform, 0.42f);

        // Sag-alt kume
        foreach (var (ox, oz, rot, path) in new (float, float, float, string)[]
            { (7.5f,7.5f,0f,lw), (8.5f,9.0f,90f,fn2), (7.0f,10.5f,45f,bw2), (9.0f,11.0f,0f,fn1) })
            Spawn(path, W(C, ox, oz), Q(rot), gBarricade.transform, 0.42f);

        // ═══════════════════════════════════════════════════════════════════
        // 5. IC KAYA GRUPLARI – arena icinde daginik
        //    PP_Rock_Pile_Moss, PP_Rock_Brown_Moss
        //    Scale: 0.27 – 0.33
        // ═══════════════════════════════════════════════════════════════════
        string[] innerPool =
        {
            SR + "PP_Rock_Pile_Moss_01.prefab", SR + "PP_Rock_Pile_Moss_02.prefab",
            SR + "PP_Rock_Pile_Moss_03.prefab", SR + "PP_Rock_Brown_Moss_01.prefab",
            SR + "PP_Rock_Brown_Moss_02.prefab", SR + "PP_Rock_Pile_01.prefab",
            SR + "PP_Rock_Pile_02.prefab",
        };
        SpawnList(innerPool, new (float, float, float)[]
        {
            ( 0.5f, -7.0f, 0.33f), ( 2.0f, -8.0f, 0.30f), ( 1.5f, -6.0f, 0.28f), // ust-orta
            (-4.5f,  0.5f, 0.30f), (-3.5f,  2.5f, 0.28f),                          // sol-orta
            ( 1.0f,  5.5f, 0.32f), (-1.5f,  7.0f, 0.29f), ( 3.5f,  6.5f, 0.27f), // alt-orta
            ( 6.5f, -1.0f, 0.28f), ( 5.0f,  0.5f, 0.30f),                          // sag-ic
        }, gInnerRock.transform, C);

        // ═══════════════════════════════════════════════════════════════════
        // 6. TIRMANMA BITKILERI – perimeter kayalarin yani + golet kenari
        //    PP_Climbing_Plant_01-06  Scale: 0.28 – 0.38
        // ═══════════════════════════════════════════════════════════════════
        string[] vegPool =
        {
            VG + "PP_Climbing_Plant_01.prefab", VG + "PP_Climbing_Plant_02.prefab",
            VG + "PP_Climbing_Plant_03.prefab", VG + "PP_Climbing_Plant_04.prefab",
            VG + "PP_Climbing_Plant_05.prefab", VG + "PP_Climbing_Plant_06.prefab",
        };
        SpawnList(vegPool, new (float, float, float)[]
        {
            // Sol perimeter
            (-10.0f, -11.0f, 0.35f), (-10.0f, -6.0f, 0.32f), (-10.0f,  1.5f, 0.38f),
            (-10.0f,   6.5f, 0.34f), (-10.0f, 11.5f, 0.36f),
            // Sag perimeter
            ( 10.0f, -11.0f, 0.33f), ( 10.0f, -5.0f, 0.36f), ( 10.0f,  2.0f, 0.32f),
            ( 10.0f,   8.0f, 0.37f),
            // Ust perimeter
            ( -7.0f, -12.0f, 0.34f), ( -2.0f, -12.0f, 0.31f), (  4.0f, -12.0f, 0.36f),
            (  8.0f, -12.0f, 0.33f),
            // Alt perimeter
            ( -7.0f,  12.0f, 0.35f), (  0.0f,  12.0f, 0.32f), (  7.0f,  12.0f, 0.36f),
            // Golet kenari
            ( -8.0f,  -6.5f, 0.30f), ( -6.5f,  -8.5f, 0.28f),
            // Ic kaya yanlari
            (  0.5f,  -7.5f, 0.28f), ( -4.0f,   0.0f, 0.30f), (  1.5f,   5.0f, 0.29f),
        }, gVeg.transform, C);

        // ═══════════════════════════════════════════════════════════════════
        // 7. SOL GIRIS KEMERI
        //    Resimde: sol kenarda ahsap portal/kemer
        //    PP_Mine_Entrance_Wooden_01  Scale: 0.40
        // ═══════════════════════════════════════════════════════════════════
        Spawn(CV + "PP_Mine_Entrance_Wooden_01.prefab",
              W(C, -10.5f, -2.5f), Q(90f), gEntrance.transform, 0.40f);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[NewMap3Layout] Harita hazir! Merkez: " + C);
        Selection.activeGameObject = root;
    }

    // ── Yardimcilar ────────────────────────────────────────────────────────
    static void SpawnList(string[] pool, (float ox, float oz, float sc)[] pts,
                          Transform parent, Vector3 C)
    {
        for (int i = 0; i < pts.Length; i++)
        {
            var (ox, oz, sc) = pts[i];
            Spawn(pool[i % pool.Length], W(C, ox, oz),
                  Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent, sc);
        }
    }

    static Vector3 W(Vector3 C, float ox, float oz)
        => new Vector3(C.x + ox, C.y, C.z + oz);

    static Quaternion Q(float yDeg) => Quaternion.Euler(0, yDeg, 0);

    static GameObject Child(GameObject p, string name)
    {
        var g = new GameObject(name);
        g.transform.SetParent(p.transform);
        return g;
    }

    static GameObject Spawn(string path, Vector3 pos, Quaternion rot,
                            Transform parent, float scale)
    {
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (pf == null) { Debug.LogWarning("[NewMap3Layout] Bulunamadi: " + path); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf);
        go.transform.position   = pos;
        go.transform.rotation   = rot;
        go.transform.localScale = Vector3.one * scale;
        go.transform.SetParent(parent);
        return go;
    }
}

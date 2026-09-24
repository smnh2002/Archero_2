using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ===========================================================================
// Crystal Mine Harita Generator
// Ground boyutu: X=23.04  Z=25.9
// Ground merkezi dünya koordinatı: (-1.18, 0.17, 3.21)
// Tüm offset'ler bu merkeze göre tanımlanmıştır.
// ===========================================================================
public class CrystalMineGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Crystal Mine (NewMap3Scene)")]
    public static void GenerateMap()
    {
        if (SceneManager.GetActiveScene().name != "NewMap3Scene")
        {
            if (EditorUtility.DisplayDialog("Yanlış Sahne",
                "'NewMap3Scene' sahnesinde çalıştırmalısınız. Açılsın mı?", "Evet", "Hayır"))
                EditorSceneManager.OpenScene("Assets/Scenes/NewMap3Scene.unity");
            else return;
        }

        // ── Eski nesilleri temizle ─────────────────────────────────────────
        var old = GameObject.Find("[Crystal_Mine_Arena]");
        if (old != null) DestroyImmediate(old);

        // ── Ground gerçek dünya merkezini bul ─────────────────────────────
        GameObject groundObj = GameObject.Find("Ground");
        Vector3 C = new Vector3(-1.18f, 0.22f, 3.21f); // fallback

        if (groundObj != null)
        {
            var bc = groundObj.GetComponent<BoxCollider>();
            if (bc != null)
            {
                C = groundObj.transform.TransformPoint(bc.center);
                C.y = groundObj.transform.position.y + 0.22f;
            }
        }

        Debug.Log("[CrystalMine] Arena merkezi: " + C);

        // ── Kök hiyerarşi ─────────────────────────────────────────────────
        var root      = new GameObject("[Crystal_Mine_Arena]");
        var perimeter = Child(root, "01_Cave_Perimeter");
        var tracks    = Child(root, "02_Abandoned_Tracks");
        var flora     = Child(root, "03_Glowing_Flora");
        var ruins     = Child(root, "04_Blacksmith_Ruins");

        string B = "Assets/New assets/PurePoly/Mining_Pack/Prefabs/";
        string rock1 = B + "Stones, Rocks/PP_Rock_01.prefab";
        string rock2 = B + "Stones, Rocks/PP_Rock_02.prefab";
        string rock3 = B + "Stones, Rocks/PP_Rock_03.prefab";
        string peb1  = B + "Stones, Rocks/PP_Pebbles_01.prefab";
        string peb2  = B + "Stones, Rocks/PP_Pebbles_02.prefab";

        // ═══════════════════════════════════════════════════════════════════
        // 1. ÇEVRE KAYALIKLARI
        // Ground'un 4 kenarına boyunca dağıtılmış küçük kayalar.
        // Her rock için x,z offseti GEREKSİNİM: kenar dışı, scale 0.15-0.22
        // Arena: X [-11.52 .. +11.52]   Z [-12.95 .. +12.95]
        // ═══════════════════════════════════════════════════════════════════

        // SOL KENAR (X ≈ -10.5)  — 10 taş, Z'de eşit aralıklı
        var leftEdge = new (float ox, float oz, string path, float sc)[]
        {
            (-10.5f, -11.5f, rock1, 0.20f),
            (-10.5f,  -8.5f, rock2, 0.18f),
            (-10.5f,  -5.5f, rock3, 0.22f),
            (-10.5f,  -2.5f, rock1, 0.17f),
            (-10.5f,   0.5f, rock2, 0.21f),
            (-10.5f,   3.5f, rock3, 0.19f),
            (-10.5f,   6.0f, rock1, 0.20f),
            (-10.5f,   8.5f, rock2, 0.18f),
            (-10.5f,  11.0f, rock3, 0.22f),
            ( -9.0f, -10.0f, peb1,  0.20f), // ikinci sıra
        };
        foreach (var e in leftEdge)
            Spawn(e.path, W(C, e.ox, e.oz), RandY(), perimeter.transform, e.sc);

        // SAĞ KENAR (X ≈ +10.5)
        var rightEdge = new (float ox, float oz, string path, float sc)[]
        {
            (10.5f, -11.5f, rock2, 0.21f),
            (10.5f,  -8.5f, rock3, 0.19f),
            (10.5f,  -5.5f, rock1, 0.20f),
            (10.5f,  -2.5f, rock2, 0.22f),
            (10.5f,   0.5f, rock3, 0.18f),
            (10.5f,   3.5f, rock1, 0.21f),
            (10.5f,   6.5f, rock2, 0.19f),
            (10.5f,   9.0f, rock3, 0.22f),
            (10.5f,  11.5f, rock1, 0.17f),
            ( 9.0f, -10.5f, peb2,  0.20f),
        };
        foreach (var e in rightEdge)
            Spawn(e.path, W(C, e.ox, e.oz), RandY(), perimeter.transform, e.sc);

        // ÜST KENAR (Z ≈ -12.0)
        var topEdge = new (float ox, float oz, string path, float sc)[]
        {
            ( -9.5f, -12.0f, rock3, 0.20f),
            ( -6.5f, -12.0f, rock1, 0.19f),
            ( -3.5f, -12.0f, rock2, 0.21f),
            ( -0.5f, -12.0f, rock3, 0.18f),
            (  2.5f, -12.0f, rock1, 0.22f),
            (  5.5f, -12.0f, rock2, 0.20f),
            (  8.5f, -12.0f, rock3, 0.19f),
            ( -7.5f, -10.5f, peb1,  0.21f), // ikinci sıra
            (  4.0f, -10.5f, peb2,  0.19f),
        };
        foreach (var e in topEdge)
            Spawn(e.path, W(C, e.ox, e.oz), RandY(), perimeter.transform, e.sc);

        // ALT KENAR (Z ≈ +12.0)
        var botEdge = new (float ox, float oz, string path, float sc)[]
        {
            ( -9.5f, 12.0f, rock1, 0.21f),
            ( -6.5f, 12.0f, rock2, 0.20f),
            ( -3.5f, 12.0f, rock3, 0.18f),
            ( -0.5f, 12.0f, rock1, 0.22f),
            (  2.5f, 12.0f, rock2, 0.19f),
            (  5.5f, 12.0f, rock3, 0.21f),
            (  8.5f, 12.0f, rock1, 0.20f),
            ( -7.5f, 10.5f, peb2,  0.18f),
            (  4.5f, 10.5f, peb1,  0.20f),
        };
        foreach (var e in botEdge)
            Spawn(e.path, W(C, e.ox, e.oz), RandY(), perimeter.transform, e.sc);

        // ═══════════════════════════════════════════════════════════════════
        // 2. TERK EDİLMİŞ RAYLAR
        // Ray hattı: X ≈ -3  (sol çeyrek), Z: -12'den +12'ye
        // Scale: ray=0.35  araba=0.30
        // ═══════════════════════════════════════════════════════════════════
        string railA  = B + "Rails and Mine Carts/PP_Rail_01.prefab";
        string railB2 = B + "Rails and Mine Carts/PP_Rail_02.prefab";
        string cartA  = B + "Rails and Mine Carts/PP_Mine_Cart_01.prefab";
        string cartB  = B + "Rails and Mine Carts/PP_Mine_Cart_02.prefab";
        string entry  = B + "Rails and Mine Carts/PP_Main_Entrance_01_Wooden.prefab";

        // Raylar: Z = -11 → +11, her 1.8 birimde bir
        float[] railZs = { -11f, -9.2f, -7.4f, -5.6f, -3.8f, -2.0f, -0.2f,
                            1.6f,  3.4f,  5.2f,  7.0f,  8.8f, 10.6f };
        for (int i = 0; i < railZs.Length; i++)
        {
            float rx = -3.0f + (i % 2 == 0 ? 0f : 0.15f); // hafif zigzag
            Spawn((i % 2 == 0) ? railA : railB2,
                  W(C, rx, railZs[i]),
                  Quaternion.Euler(0, 0, 0), tracks.transform, 0.35f);
        }

        // Hatta maden arabaları (Z aralığında seçili noktalarda)
        var onTrackCarts = new (float ox, float oz, float rot)[]
        {
            (-3.0f, -9.0f,   5f),
            (-3.0f, -3.5f,  -8f),
            (-3.0f,  2.5f,   3f),
            (-3.0f,  8.0f, -10f),
        };
        foreach (var oc in onTrackCarts)
            Spawn(cartA, W(C, oc.ox, oc.oz), Quaternion.Euler(0, oc.rot, 0), tracks.transform, 0.30f);

        // Hattan çıkmış / devrilmiş arabalar — farklı konumlarda
        var looseCarts = new (float ox, float oz, float rot, string path)[]
        {
            ( 5.5f,  -8.0f, 45f,  cartB),
            ( 7.0f,   2.0f, 130f, cartA),
            (-6.0f,   6.5f, 220f, cartB),
            ( 4.0f,   9.5f, 310f, cartA),
            (-7.5f,  -4.0f,  80f, cartB),
        };
        foreach (var lc in looseCarts)
            Spawn(lc.path, W(C, lc.ox, lc.oz), Quaternion.Euler(0, lc.rot, 0), tracks.transform, 0.30f);

        // Maden girişi — üst kenara yakın, ortaya
        Spawn(entry, W(C, 0f, -11.5f), Quaternion.Euler(0, 0, 0), tracks.transform, 0.40f);

        // ═══════════════════════════════════════════════════════════════════
        // 3. PARLAYAN FLORA
        // Kristaller ve mantarlar — sabit, geniş alana yayılmış pozisyonlar
        // Scale: 0.22 – 0.32
        // ═══════════════════════════════════════════════════════════════════
        string cBlue  = B + "Ores and Crystals/PP_Crystal_01_Blue.prefab";
        string cGreen = B + "Ores and Crystals/PP_Crystal_01_Green.prefab";
        string cRed   = B + "Ores and Crystals/PP_Crystal_01_Red.prefab";
        string cGold  = B + "Ores and Crystals/PP_Crystal_01_Gold.prefab";
        string mBr1   = B + "Mushrooms/PP_Mushrooms_Brown_01.prefab";
        string mBr2   = B + "Mushrooms/PP_Mushrooms_Brown_02.prefab";
        string mBr3   = B + "Mushrooms/PP_Mushrooms_Brown_03.prefab";

        // Her satır: (worldX_offset, worldZ_offset, scale, prefab, lightColor)
        var floraPoints = new (float ox, float oz, float sc, string path, Color lc)[]
        {
            // === SOL BÖLGE (X: -9 .. -5) ===
            ( -8.5f, -10.0f, 0.30f, cBlue,  Color.cyan),
            ( -7.0f,  -6.5f, 0.25f, cGreen, Color.green),
            ( -8.0f,  -1.5f, 0.32f, cBlue,  Color.cyan),
            ( -6.5f,   3.5f, 0.27f, cRed,   Color.red),
            ( -8.5f,   8.5f, 0.29f, cGold,  new Color(1f,0.85f,0.1f)),
            ( -5.5f,   0.0f, 0.24f, mBr1,   Color.green),
            ( -6.0f,  -4.0f, 0.28f, mBr2,   Color.green),

            // === SAĞ BÖLGE (X: +5 .. +9) ===
            (  8.0f, -10.5f, 0.28f, cGreen, Color.green),
            (  7.5f,  -5.0f, 0.32f, cBlue,  Color.cyan),
            (  8.5f,   1.0f, 0.25f, cRed,   Color.red),
            (  7.0f,   6.5f, 0.30f, cGold,  new Color(1f,0.85f,0.1f)),
            (  5.5f,  -2.5f, 0.26f, mBr3,   Color.green),
            (  6.0f,   3.5f, 0.27f, mBr1,   Color.green),

            // === ÜST BÖLGE (Z: -9 .. -6) ===
            ( -3.0f,  -9.5f, 0.26f, mBr2, Color.green),
            (  1.5f,  -8.5f, 0.29f, cRed, Color.red),
            (  5.0f,  -9.0f, 0.25f, mBr3, Color.green),

            // === ALT BÖLGE (Z: +7 .. +11) ===
            ( -4.0f,   9.5f, 0.28f, mBr1,  Color.green),
            (  1.5f,  10.5f, 0.30f, cBlue, Color.cyan),
            (  5.5f,   8.5f, 0.26f, mBr2,  Color.green),

            // === ORTA-SAĞ ÇEYREĞİ (ray hattından sağda) ===
            (  2.5f,  -5.0f, 0.27f, cGreen, Color.green),
            (  4.0f,   0.5f, 0.30f, cBlue,  Color.cyan),
            (  3.0f,   5.5f, 0.25f, cRed,   Color.red),
        };

        foreach (var fp in floraPoints)
        {
            var obj = Spawn(fp.path, W(C, fp.ox, fp.oz),
                            Quaternion.Euler(0, Random.Range(0f, 360f), 0),
                            flora.transform, fp.sc);
            if (obj != null)
            {
                var lt      = obj.AddComponent<Light>();
                lt.type     = LightType.Point;
                lt.color    = fp.lc;
                lt.intensity= 1.2f;
                lt.range    = 4.0f;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // 4. DEMİRCİ YIKINTILAR — sağ-üst köşe (X≈+7  Z≈+8)
        // Scale: duvar 0.35   prop 0.25-0.30
        // ═══════════════════════════════════════════════════════════════════
        float rx2 =  7.0f;  // ruin X offset from center
        float rz2 =  8.0f;  // ruin Z offset

        string wA  = B + "Blacksmith House/PP_Wooden_Wall_01.prefab";
        string wB2 = B + "Blacksmith House/PP_Wooden_Wall_02.prefab";
        string flr = B + "Blacksmith House/PP_Wooden_Floor_01.prefab";
        string anv = B + "Tools/PP_Anvil_New_01_New_Iron.prefab";
        string brr = B + "Props/PP_Barrel_01.prefab";
        string crt = B + "Props/PP_Crate_Wooden_01.prefab";
        string trc = B + "Tools/PP_Torch_Standing_01.prefab";

        // Zemin
        Spawn(flr, W(C, rx2,        rz2),        Quaternion.identity,        ruins.transform, 0.35f);
        Spawn(flr, W(C, rx2 + 1.0f, rz2),        Quaternion.identity,        ruins.transform, 0.35f);
        Spawn(flr, W(C, rx2,        rz2 + 1.0f), Quaternion.identity,        ruins.transform, 0.35f);
        Spawn(flr, W(C, rx2 + 1.0f, rz2 + 1.0f), Quaternion.identity,       ruins.transform, 0.35f);

        // Duvarlar (köşenin etrafı)
        Spawn(wA,  W(C, rx2 + 1.5f, rz2 + 0.5f), Quaternion.Euler(0,  90, 0), ruins.transform, 0.35f);
        Spawn(wA,  W(C, rx2 - 1.5f, rz2 + 0.5f), Quaternion.Euler(0, 270, 0), ruins.transform, 0.35f);
        Spawn(wB2, W(C, rx2 + 0.5f, rz2 + 2.0f), Quaternion.Euler(0,   0, 0), ruins.transform, 0.35f);

        // Props — her biri farklı konumda
        Spawn(anv, W(C, rx2 + 0.3f, rz2 + 0.3f), Quaternion.Euler(0,  20, 0), ruins.transform, 0.28f);
        Spawn(brr, W(C, rx2 - 1.0f, rz2 + 0.7f), Quaternion.Euler(0,  55, 0), ruins.transform, 0.26f);
        Spawn(brr, W(C, rx2 - 1.2f, rz2 - 0.5f), Quaternion.Euler(0, 130, 0), ruins.transform, 0.26f);
        Spawn(crt, W(C, rx2 + 1.3f, rz2 + 0.8f), Quaternion.Euler(0, 210, 0), ruins.transform, 0.28f);
        Spawn(crt, W(C, rx2 + 1.2f, rz2 - 0.7f), Quaternion.Euler(0,  75, 0), ruins.transform, 0.28f);

        // Meşaleler — köşenin iki yanına
        Spawn(trc, W(C, rx2 + 2.0f, rz2 + 1.5f), Quaternion.Euler(0,   0, 0), ruins.transform, 0.30f);
        Spawn(trc, W(C, rx2 - 2.0f, rz2 + 1.5f), Quaternion.Euler(0, 180, 0), ruins.transform, 0.30f);

        // Demirci kristali — ışık efekti
        var rc = Spawn(cBlue, W(C, rx2 - 2.0f, rz2 - 1.5f),
                       Quaternion.Euler(0, 135, 0), ruins.transform, 0.32f);
        if (rc != null)
        {
            var lt = rc.AddComponent<Light>();
            lt.type = LightType.Point; lt.color = Color.cyan;
            lt.intensity = 1.8f; lt.range = 6f;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[CrystalMine] ✅ Harita hazır! Arena merkezi: " + C);
        Selection.activeGameObject = root;
    }

    // ── Yardımcı: merkeze göre world pozisyonu ─────────────────────────────
    static Vector3 W(Vector3 center, float ox, float oz)
        => new Vector3(center.x + ox, center.y, center.z + oz);

    static Quaternion RandY() => Quaternion.Euler(0, Random.Range(0f, 360f), 0);

    // ── Yardımcı: child ───────────────────────────────────────────────────
    static GameObject Child(GameObject p, string name)
    {
        var g = new GameObject(name);
        g.transform.SetParent(p.transform);
        return g;
    }

    // ── Yardımcı: prefab spawn ─────────────────────────────────────────────
    static GameObject Spawn(string path, Vector3 pos, Quaternion rot, Transform parent, float scale)
    {
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (pf == null) { Debug.LogWarning("[CrystalMine] Bulunamadı: " + path); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf);
        go.transform.position   = pos;
        go.transform.rotation   = rot;
        go.transform.localScale = Vector3.one * scale;
        go.transform.SetParent(parent);
        return go;
    }
}

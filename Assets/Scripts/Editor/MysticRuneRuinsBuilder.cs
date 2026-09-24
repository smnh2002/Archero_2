#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// NewMapScene icin "Antik Runik Tapinak Harabeleri (Mystic Rune Ruins)" temasinda
/// cevreyi, siperleri, run cemberini, heykelleri ve efektleri otomatik kuran Editor araci.
/// 
/// Menu: Tools > Mystic Rune Ruins > Build Complete Mystic Rune Ruins
/// </summary>
public static class MysticRuneRuinsBuilder
{
    // ── Ground Sinirlari & Merkez (NewMapScene ground olculeri) ───────────
    private const float GROUND_MIN_X = -18.0f;
    private const float GROUND_MAX_X =  88.0f;
    private const float GROUND_MIN_Z = -86.0f;
    private const float GROUND_MAX_Z =  64.0f;

    private const float CENTER_X =  35.0f;
    private const float CENTER_Z = -11.0f;
    private const float GROUND_Y =   0.0f;

    private const string BASE = "Assets/New assets/PurePoly/Mining_Pack/Prefabs";
    private const string ROOT_NAME = "[Mystic_Rune_Ruins]";

    // ── Menu Butonlari ───────────────────────────────────────────────────

    [MenuItem("Tools/Mystic Rune Ruins/Build Complete Mystic Rune Ruins (TEK TIKLA KUR)")]
    public static void BuildCompleteRuins()
    {
        if (!EditorUtility.DisplayDialog("Antik Runik Tapinak Kurulumu",
            "NewMapScene uzerine 'Antik Runik Tapinak Harabeleri' duzeni kurulacak:\n\n" +
            "• Merkezde Buyulu Runik Ayin Cemberi (12 Run Tasi + Mystic FX)\n" +
            "• 4 Kosede Atesli ve Kristalli Muhafiz Sutunlari (Siper)\n" +
            "• Kuzeyde Kadim Sunak, Heykeller ve Hazine Sandigi\n" +
            "• Dogu ve Batida Taktiksel Kaya/Kristal Siperleri\n" +
            "• Guneyde Harabe Giris Kapisi ve Tas Yol\n" +
            "• Harita Sinirlarinda Antik Ucurum ve Sutun Duvarlari\n" +
            "• Mistik Zemin Sisi ve Aydinlatma Ayari\n\n" +
            "Ctrl+Z ile istediginiz an geri alabilirsiniz. Devam edilsin mi?",
            "Evet, Tapinagi Insa Et", "Iptal"))
            return;

        Undo.SetCurrentGroupName("Build Mystic Rune Ruins");
        int group = Undo.GetCurrentGroup();

        ClearOldRuins(false);

        GameObject root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Create Ruins Root");

        // 1. Merkez: Büyülü Rünik Ayin Meydanı
        BuildCentralRitualCircle(root);

        // 2. Dört Köşe: Muhafız Sütunları ve Siperler
        BuildGuardianPillars(root);

        // 3. Kuzey: Kadim Sunak ve Heykeller
        BuildNorthSanctumAltar(root);

        // 4. Doğu & Batı: Taktiksel Siper Kümeleri (Kite / Siper)
        BuildTacticalCoverClusters(root);

        // 5. Güney: Harabe Giriş Kapısı & Taş Yol
        BuildSouthGatewayRuins(root);

        // 6. Dış Çevre: Uçurum ve Sütun Duvarları (Sınırlar)
        BuildPerimeterCliffsAndColumns(root);

        // 7. Atmosferik Zemin Sisi
        BuildAtmosphericFog(root);

        // 8. Işıklandırmayı Mistik Gece/Alacakaranlık moduna ayarla
        ApplyMysticLighting();

        // 9. NavMesh'i otomatik güncelle
        BakeNavMeshSurface();

        Undo.CollapseUndoOperations(group);

        Debug.Log("<color=cyan><b>[MysticRuneRuinsBuilder]</b> Antik Runik Tapinak Harabeleri basariyla insa edildi ve NavMesh guncellendi!</color>");
        EditorUtility.DisplayDialog("Basarili!",
            "Antik Runik Tapinak Harabeleri basariyla insa edildi!\n\n" +
            "• NavMesh otomatik olarak yeni engellere gore bake edildi.\n" +
            "• Begenmezseniz Tools > Mystic Rune Ruins > Clear ile temizleyebilirsiniz.",
            "Harika!");
    }

    [MenuItem("Tools/Mystic Rune Ruins/Bake NavMesh (Yeniden Hesapla)")]
    public static void BakeNavMeshMenu()
    {
        BakeNavMeshSurface();
    }

    [MenuItem("Tools/Mystic Rune Ruins/Set Mystic Lighting (Mistik Isiklandirma)")]
    public static void SetLightingMenu()
    {
        ApplyMysticLighting();
        Debug.Log("<color=cyan>[MysticRuneRuinsBuilder] Mistik mavi/mor aydinlatma uygulandi.</color>");
    }

    [MenuItem("Tools/Mystic Rune Ruins/Reduce Fog Density (Sisi Azalt)")]
    public static void ReduceFogMenu()
    {
        ReduceFogInCurrentScene();
    }

    [MenuItem("Tools/Mystic Rune Ruins/Clear Mystic Rune Ruins (Temizle)")]
    public static void ClearMenu()
    {
        ClearOldRuins(true);
    }

    // ── 1. Merkez Ayin Çemberi ──────────────────────────────────────────
    private static void BuildCentralRitualCircle(GameObject root)
    {
        GameObject parent = CreateChild(root, "01_Central_Ritual_Circle");

        // Büyük parlayan rünik çember (Kullanıcı yerde mavi çizgiler olarak şikayet ettiği için kapatıldı)
        // Spawn(BASE + "/FX/Mystic/FX_Mystic Ring_Large_01.prefab",
        //     new Vector3(CENTER_X, GROUND_Y + 0.05f, CENTER_Z), 0f, 1.6f, parent);

        // Ortada hafif parlayan mistik ışıltı (İsteğe bağlı kapatıldı)
        // Spawn(BASE + "/FX/Mystic/FX_Mystic_Shine_01.prefab",
        //     new Vector3(CENTER_X, GROUND_Y + 0.1f, CENTER_Z), 0f, 1.0f, parent);

        // 12 Adet Rün Taşı (Dairesel 9 metre yarıçapta)
        float radius = 9.0f;
        for (int i = 0; i < 12; i++)
        {
            float angleDeg = i * 30.0f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float x = CENTER_X + Mathf.Sin(angleRad) * radius;
            float z = CENTER_Z + Mathf.Cos(angleRad) * radius;

            string runeIndex = (i + 1).ToString("D2");
            string runePrefab = $"{BASE}/Pillars, Runes/PP_Rune_Stone_{runeIndex}.prefab";

            // Merkeze baksın
            float rotY = angleDeg + 180f;
            Spawn(runePrefab, new Vector3(x, GROUND_Y, z), rotY, 1.15f, parent);
        }
    }

    // ── 2. Dört Köşe Muhafız Sütunları ──────────────────────────────────
    private static void BuildGuardianPillars(GameObject root)
    {
        GameObject parent = CreateChild(root, "02_Guardian_Pillars");

        // KB, KD, GB, GD köşe koordinatları (Merkezden 18m X, 20m Z uzaklıkta)
        Vector2[] offsets = {
            new Vector2(-18.0f,  20.0f), // Kuzeybatı
            new Vector2( 18.0f,  20.0f), // Kuzeydoğu
            new Vector2(-18.0f, -20.0f), // Güneybatı
            new Vector2( 18.0f, -20.0f)  // Güneydoğu
        };

        string[] pillarPrefabs = {
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_01.prefab",
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_02.prefab",
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_03.prefab",
            BASE + "/Pillars, Runes/PP_Pillar_Stone_Rune_01.prefab"
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 pos = new Vector3(CENTER_X + offsets[i].x, GROUND_Y, CENTER_Z + offsets[i].y);

            // Ana Rünik Sütun
            Spawn(pillarPrefabs[i], pos, i * 90f, 1.25f, parent);

            // Sütun tepesine veya yanına Mistik Mavi Alev
            Spawn(BASE + "/FX/Mystic/FX_Magic_Fire_01.prefab", pos + new Vector3(0, 3.8f, 0), 0f, 1.0f, parent);

            // Sütun dibine devrilmiş parça veya küçük rün sütunu
            Vector3 brokenOffset = new Vector3(Mathf.Sign(offsets[i].x) * 2.2f, 0, Mathf.Sign(offsets[i].y) * -1.5f);
            Spawn(BASE + "/Pillars, Runes/PP_Pillar_Stone_04.prefab", pos + brokenOffset, (i * 45f) + 30f, 1.0f, parent);

            // Mavi kristal kümesi
            Vector3 crystalOffset = new Vector3(Mathf.Sign(offsets[i].x) * -1.8f, 0, Mathf.Sign(offsets[i].y) * 1.8f);
            Spawn(BASE + "/Ores and Crystals/PP_Crystal_Cluster_01_Blue.prefab", pos + crystalOffset, i * 60f, 1.1f, parent);
        }
    }

    // ── 3. Kuzey Kadim Sunak ve Heykeller ────────────────────────────────
    private static void BuildNorthSanctumAltar(GameObject root)
    {
        GameObject parent = CreateChild(root, "03_North_Sanctum_Altar");

        float altarZ = CENTER_Z + 36.0f;

        // Kadim Dikili Taşlar (Menhirs) - Sunak Arka Duvarı
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_Moss_01.prefab", new Vector3(CENTER_X, GROUND_Y, altarZ + 4.0f), 0f, 1.5f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_02.prefab", new Vector3(CENTER_X - 4.5f, GROUND_Y, altarZ + 3.0f), 25f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_02.prefab", new Vector3(CENTER_X + 4.5f, GROUND_Y, altarZ + 3.0f), -25f, 1.3f, parent);

        // Sunak Koruyucu Heykelleri (Sağ ve Sol Muhafız)
        Spawn(BASE + "/Props/PP_Stone_Statue_01.prefab", new Vector3(CENTER_X - 10.0f, GROUND_Y, altarZ), 140f, 1.25f, parent);
        Spawn(BASE + "/Props/PP_Stone_Statue_02.prefab", new Vector3(CENTER_X + 10.0f, GROUND_Y, altarZ), -140f, 1.25f, parent);

        // Heykellerin önüne meşaleler
        Spawn(BASE + "/Props/PP_Torch_Standing_01.prefab", new Vector3(CENTER_X - 10.0f, GROUND_Y, altarZ - 3.0f), 0f, 1.1f, parent);
        Spawn(BASE + "/Props/PP_Torch_Standing_01.prefab", new Vector3(CENTER_X + 10.0f, GROUND_Y, altarZ - 3.0f), 0f, 1.1f, parent);

        // Sunak merkezindeki küçük mistik halka ve hazine sandığı
        // Spawn(BASE + "/FX/Mystic/FX_Mystic Ring_01.prefab", new Vector3(CENTER_X, GROUND_Y + 0.05f, altarZ), 0f, 1.3f, parent);
        Spawn(BASE + "/Props/PP_Treasure_Chest_01_Blue.prefab", new Vector3(CENTER_X, GROUND_Y, altarZ), 180f, 1.2f, parent);
    }

    // ── 4. Doğu & Batı Taktiksel Siper Kümeleri ──────────────────────────
    private static void BuildTacticalCoverClusters(GameObject root)
    {
        GameObject parent = CreateChild(root, "04_Tactical_Cover_Clusters");

        // BATI Siper Kümesi (X: 10.0, Z: -11.0 civarı)
        Vector3 westPos = new Vector3(CENTER_X - 25.0f, GROUND_Y, CENTER_Z);
        Spawn(BASE + "/Stones, Rocks/PP_Stone_Column_01.prefab", westPos, 15f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_01.prefab", westPos + new Vector3(2.5f, 0, 1.5f), 45f, 1.2f, parent);
        Spawn(BASE + "/Ores and Crystals/PP_Crystal_Column_01_Blue.prefab", westPos + new Vector3(-1.8f, 0, -2.0f), 70f, 1.2f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_01.prefab", westPos + new Vector3(0.5f, 0, -3.0f), 0f, 1.0f, parent);

        // DOĞU Siper Kümesi (X: 60.0, Z: -11.0 civarı)
        Vector3 eastPos = new Vector3(CENTER_X + 25.0f, GROUND_Y, CENTER_Z);
        Spawn(BASE + "/Stones, Rocks/PP_Stone_Column_02.prefab", eastPos, -20f, 1.3f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_02.prefab", eastPos + new Vector3(-2.5f, 0, -1.5f), 120f, 1.2f, parent);
        Spawn(BASE + "/Ores and Crystals/PP_Crystal_Column_02_Blue.prefab", eastPos + new Vector3(1.8f, 0, 2.0f), -45f, 1.2f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Pile_03.prefab", eastPos + new Vector3(-0.5f, 0, 3.0f), 0f, 1.0f, parent);

        // KUZEY-BATI ve GÜNEY-DOĞU ek küçük kaya siperleri
        Spawn(BASE + "/Stones, Rocks/PP_Rock_06.prefab", new Vector3(CENTER_X - 12.0f, GROUND_Y, CENTER_Z + 12.0f), 30f, 1.1f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_06.prefab", new Vector3(CENTER_X + 12.0f, GROUND_Y, CENTER_Z - 12.0f), -60f, 1.1f, parent);
    }

    // ── 5. Güney Harabe Giriş Kapısı & Taş Yol ──────────────────────────
    private static void BuildSouthGatewayRuins(GameObject root)
    {
        GameObject parent = CreateChild(root, "05_South_Gateway_Ruins");

        float gateZ = CENTER_Z - 38.0f;

        // İki ana giriş kapı sütunu
        Spawn(BASE + "/Pillars, Runes/PP_Pillar_Stone_01.prefab", new Vector3(CENTER_X - 6.0f, GROUND_Y, gateZ), 0f, 1.35f, parent);
        Spawn(BASE + "/Pillars, Runes/PP_Pillar_Stone_01.prefab", new Vector3(CENTER_X + 6.0f, GROUND_Y, gateZ), 0f, 1.35f, parent);

        // Kapı meşaleleri
        Spawn(BASE + "/Props/PP_Torch_Standing_01.prefab", new Vector3(CENTER_X - 4.5f, GROUND_Y, gateZ + 2.0f), 0f, 1.1f, parent);
        Spawn(BASE + "/Props/PP_Torch_Standing_01.prefab", new Vector3(CENTER_X + 4.5f, GROUND_Y, gateZ + 2.0f), 0f, 1.1f, parent);

        // Kapı yanlarına yıkık taşlar
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_01.prefab", new Vector3(CENTER_X - 10.0f, GROUND_Y, gateZ - 1.0f), -45f, 1.1f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Rock_Menhir_01.prefab", new Vector3(CENTER_X + 10.0f, GROUND_Y, gateZ - 1.0f), 45f, 1.1f, parent);

        // Merkeze doğru uzanan taş yol parçaları
        Spawn(BASE + "/Stones, Rocks/PP_Stone_Path_01.prefab", new Vector3(CENTER_X, GROUND_Y + 0.02f, gateZ + 5.0f), 0f, 1.2f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Stone_Path_02.prefab", new Vector3(CENTER_X, GROUND_Y + 0.02f, gateZ + 12.0f), 180f, 1.2f, parent);
        Spawn(BASE + "/Stones, Rocks/PP_Stone_Path_01.prefab", new Vector3(CENTER_X, GROUND_Y + 0.02f, gateZ + 19.0f), 0f, 1.2f, parent);
    }

    // ── 6. Dış Çevre: Uçurum ve Sütun Duvarları (Sınırlar) ───────────────
    private static void BuildPerimeterCliffsAndColumns(GameObject root)
    {
        GameObject parent = CreateChild(root, "06_Perimeter_Boundaries");

        // KUZEY SINIRI (Z: ~52)
        for (float x = GROUND_MIN_X + 4f; x <= GROUND_MAX_X - 4f; x += 14.0f)
        {
            string cliff = (Mathf.Abs(x) % 2 == 0) ? BASE + "/Cave/PP_Cliff_01.prefab" : BASE + "/Cave/PP_Cliff_02.prefab";
            Spawn(cliff, new Vector3(x, GROUND_Y, GROUND_MAX_Z - 8.0f), 180f, 1.2f, parent);
        }

        // GÜNEY SINIRI (Z: ~ -78)
        for (float x = GROUND_MIN_X + 4f; x <= GROUND_MAX_X - 4f; x += 14.0f)
        {
            string cliff = (Mathf.Abs(x) % 2 == 0) ? BASE + "/Cave/PP_Cliff_02.prefab" : BASE + "/Cave/PP_Cliff_03.prefab";
            Spawn(cliff, new Vector3(x, GROUND_Y, GROUND_MIN_Z + 6.0f), 0f, 1.2f, parent);
        }

        // BATI SINIRI (X: ~ -14)
        for (float z = GROUND_MIN_Z + 16f; z <= GROUND_MAX_Z - 16f; z += 14.0f)
        {
            Spawn(BASE + "/Cave/PP_Cliff_01.prefab", new Vector3(GROUND_MIN_X + 4.0f, GROUND_Y, z), 90f, 1.2f, parent);
            Spawn(BASE + "/Stones, Rocks/PP_Rock_Column_01.prefab", new Vector3(GROUND_MIN_X + 6.0f, GROUND_Y, z + 5.0f), 0f, 1.1f, parent);
        }

        // DOĞU SINIRI (X: ~ 82)
        for (float z = GROUND_MIN_Z + 16f; z <= GROUND_MAX_Z - 16f; z += 14.0f)
        {
            Spawn(BASE + "/Cave/PP_Cliff_03.prefab", new Vector3(GROUND_MAX_X - 4.0f, GROUND_Y, z), -90f, 1.2f, parent);
            Spawn(BASE + "/Stones, Rocks/PP_Rock_Column_02.prefab", new Vector3(GROUND_MAX_X - 6.0f, GROUND_Y, z + 5.0f), 0f, 1.1f, parent);
        }
    }

    // ── 7. Atmosferik Zemin Sisi (Hafif ve Düşük Yoğunluk) ──────────────
    private static void BuildAtmosphericFog(GameObject root)
    {
        GameObject parent = CreateChild(root, "07_Atmosphere_FX");
        SpawnSubtleFog(parent);
    }

    private static void SpawnSubtleFog(GameObject parent)
    {
        // Devasa ve opak FX_Fog_Big yerine çok daha şeffaf ve tabana yakın FX_Fog_01
        Vector3[] fogPositions = {
            new Vector3(CENTER_X, GROUND_Y, CENTER_Z),
            new Vector3(CENTER_X, GROUND_Y, CENTER_Z + 25.0f)
        };

        foreach (var pos in fogPositions)
        {
            Spawn(BASE + "/FX/Fog/FX_Fog_01.prefab", pos, 0f, 0.65f, parent);
        }
    }

    private static void ReduceFogInCurrentScene()
    {
        var ruins = GameObject.Find(ROOT_NAME);
        Transform atmo = null;
        if (ruins != null)
        {
            atmo = ruins.transform.Find("07_Atmosphere_FX");
        }
        else
        {
            var atmoGo = GameObject.Find("07_Atmosphere_FX");
            if (atmoGo != null) atmo = atmoGo.transform;
        }

        if (atmo != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(atmo.gameObject, "Reduce Fog");
            var toDestroy = new List<GameObject>();
            for (int i = 0; i < atmo.childCount; i++)
            {
                toDestroy.Add(atmo.GetChild(i).gameObject);
            }
            foreach (var go in toDestroy)
            {
                Undo.DestroyObjectImmediate(go);
            }

            SpawnSubtleFog(atmo.gameObject);

            Debug.Log("<color=lime>[MysticRuneRuinsBuilder] Sis yogunlugu basariyla hafifletildi!</color>");
            EditorUtility.DisplayDialog("Sis Azaltildi", "Yogun sis bulutlari kaldirildi ve yerine zemin seviyesinde hafif 2 adet sis yerlestirildi.", "Tamam");
        }
        else
        {
            Debug.LogWarning("[MysticRuneRuinsBuilder] '07_Atmosphere_FX' hiyerarside bulunamadi.");
        }
    }

    // ── 8. Mistik Aydınlatma Ayarları ───────────────────────────────────
    private static void ApplyMysticLighting()
    {
        Light dirLight = null;
        var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                dirLight = l;
                break;
            }
        }

        if (dirLight != null)
        {
            Undo.RecordObject(dirLight, "Set Mystic Light Color");
            // Mistik alacakaranlık / ay ışığı mavisi
            dirLight.color = new Color(0.48f, 0.62f, 0.88f, 1f);
            dirLight.intensity = 0.95f;
            dirLight.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.22f, 1f);
    }

    // ── 9. Otomatik NavMeshSurface Bake ──────────────────────────────────
    private static void BakeNavMeshSurface()
    {
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Component navSurface = ground.GetComponent("NavMeshSurface");
            if (navSurface == null)
            {
                var allSurfaces = ground.GetComponentsInChildren(typeof(Component));
                foreach (var c in allSurfaces)
                {
                    if (c != null && c.GetType().Name == "NavMeshSurface")
                    {
                        navSurface = c;
                        break;
                    }
                }
            }

            if (navSurface != null)
            {
                MethodInfo buildMethod = navSurface.GetType().GetMethod("BuildNavMesh", BindingFlags.Public | BindingFlags.Instance);
                if (buildMethod != null)
                {
                    buildMethod.Invoke(navSurface, null);
                    Debug.Log("<color=lime>[MysticRuneRuinsBuilder] NavMeshSurface basariyla yeniden bake edildi!</color>");
                    return;
                }
            }
        }

        Debug.LogWarning("[MysticRuneRuinsBuilder] Ground objesinde NavMeshSurface bulunamadi veya BuildNavMesh cagrılamadi. Lutfen elle Ground > NavMeshSurface uzerinden Bake butonuna basiniz.");
    }

    // ── Yardımcı Fonksiyonlar ───────────────────────────────────────────

    private static void ClearOldRuins(bool showDialog)
    {
        var existing = GameObject.Find(ROOT_NAME);
        if (existing != null)
        {
            if (!showDialog || EditorUtility.DisplayDialog("Temizle", ROOT_NAME + " objesi ve tum alt elemanlari silinecek.", "Sil", "Iptal"))
            {
                Undo.DestroyObjectImmediate(existing);
                Debug.Log("<color=yellow>[MysticRuneRuinsBuilder] Onceki Tapinak objeleri temizlendi.</color>");
            }
        }
    }

    private static GameObject CreateChild(GameObject root, string name)
    {
        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create " + name);
        child.transform.SetParent(root.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static GameObject Spawn(string assetPath, Vector3 pos, float rotY, float uniformScale, GameObject parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning("[MysticRuneRuinsBuilder] Prefab bulunamadi: " + assetPath);
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

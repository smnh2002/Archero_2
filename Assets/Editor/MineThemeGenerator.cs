using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MineThemeGenerator : EditorWindow
{
    [MenuItem("Archero Tools/Generate Mine Theme")]
    public static void GenerateMineMap()
    {
        // Temiz bir başlangıç için eski Environment_Mine objesini bul ve sil
        GameObject oldRoot = GameObject.Find("Mine_Environment");
        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot);
        }

        GameObject root = new GameObject("Mine_Environment");
        Undo.RegisterCreatedObjectUndo(root, "Generate Mine Theme");

        // Prefab yolları
        string pathBase = "Assets/New assets/PurePoly/Mining_Pack/Prefabs/";
        GameObject groundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Environment/PP_Stone_Ground_01.prefab");
        GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Cave/PP_Cave_Wall_Even_01.prefab");
        GameObject railPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Rails and Mine Carts/PP_Rail_Straight_01.prefab");
        GameObject cartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Rails and Mine Carts/PP_Mine_Cart_01.prefab");
        GameObject woodenSupportPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Cave/PP_Mine_Wooden_Support_01.prefab");
        GameObject crystalBlue = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Ores and Crystals/PP_Crystal_Cluster_01_Blue.prefab");
        GameObject crystalRed = AssetDatabase.LoadAssetAtPath<GameObject>(pathBase + "Ores and Crystals/PP_Crystal_Cluster_01_Red.prefab");

        if (groundPrefab == null)
        {
            Debug.LogError("Prefabs could not be loaded. Please check the paths.");
            return;
        }

        // Objelerin boyutlarını tespit etmek için geçici olarak yaratıp ölçelim
        GameObject tempGround = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
        Renderer groundRend = tempGround.GetComponentInChildren<Renderer>();
        float groundSizeX = groundRend != null ? groundRend.bounds.size.x : 5f;
        float groundSizeZ = groundRend != null ? groundRend.bounds.size.z : 5f;
        DestroyImmediate(tempGround);

        GameObject tempWall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
        Renderer wallRend = tempWall.GetComponentInChildren<Renderer>();
        float wallSizeX = wallRend != null ? wallRend.bounds.size.x : 5f;
        DestroyImmediate(tempWall);

        // Düzenli bir Maden Tüneli oluşturalım (Düz bir koridor)
        int tunnelLength = 8; // 8 parça uzunluğunda

        for (int z = 0; z < tunnelLength; z++)
        {
            Vector3 centerPos = new Vector3(0, 0, z * groundSizeZ);

            // 1. Zemin (Orta, Sol ve Sağ için 3 parça genişliğinde yapalım)
            for (int x = -1; x <= 1; x++)
            {
                Vector3 groundPos = centerPos + new Vector3(x * groundSizeX, 0, 0);
                GameObject ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
                ground.transform.position = groundPos;
                ground.transform.parent = root.transform;

                // 2. Duvarlar (Sadece en sol ve en sağa)
                if (x == -1)
                {
                    GameObject wallL = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
                    wallL.transform.position = groundPos + new Vector3(-groundSizeX / 2f + wallSizeX / 2f, 0, 0);
                    wallL.transform.rotation = Quaternion.Euler(0, 90, 0);
                    wallL.transform.parent = root.transform;

                    // Bazen duvara kristal ekleyelim
                    if (z % 2 == 0 && crystalBlue != null)
                    {
                        GameObject crystal = (GameObject)PrefabUtility.InstantiatePrefab(crystalBlue);
                        crystal.transform.position = wallL.transform.position + new Vector3(1f, 0.5f, 0);
                        crystal.transform.parent = root.transform;
                    }
                }
                else if (x == 1)
                {
                    GameObject wallR = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
                    wallR.transform.position = groundPos + new Vector3(groundSizeX / 2f - wallSizeX / 2f, 0, 0);
                    wallR.transform.rotation = Quaternion.Euler(0, -90, 0);
                    wallR.transform.parent = root.transform;

                    if (z % 3 == 0 && crystalRed != null)
                    {
                        GameObject crystal = (GameObject)PrefabUtility.InstantiatePrefab(crystalRed);
                        crystal.transform.position = wallR.transform.position + new Vector3(-1f, 0.5f, 0);
                        crystal.transform.parent = root.transform;
                    }
                }
            }

            // 3. Merkeze Rayları Döşeyelim
            if (railPrefab != null)
            {
                GameObject rail = (GameObject)PrefabUtility.InstantiatePrefab(railPrefab);
                rail.transform.position = centerPos + new Vector3(0, 0.05f, 0); // Yerden hafif yüksek
                rail.transform.parent = root.transform;

                // Tünelin ortasında bir maden arabası olsun
                if (z == tunnelLength / 2 && cartPrefab != null)
                {
                    GameObject cart = (GameObject)PrefabUtility.InstantiatePrefab(cartPrefab);
                    cart.transform.position = rail.transform.position + new Vector3(0, 0, 0);
                    cart.transform.parent = root.transform;
                }
            }

            // 4. Ahşap Destekleri (Wooden Supports) belirli aralıklarla ekleyelim
            if (z % 2 == 1 && woodenSupportPrefab != null)
            {
                GameObject support = (GameObject)PrefabUtility.InstantiatePrefab(woodenSupportPrefab);
                support.transform.position = centerPos;
                support.transform.parent = root.transform;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Mine map successfully generated with organized layout!");
    }
}

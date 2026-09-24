using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eski sahnenin havuzlanmis objeleri sahne degisince yok oldu,
        // referanslari temizleyip sifirdan basliyoruz.
        pools.Clear();
        instanceToPrefab.Clear();
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject instance;

        if (queue.Count > 0)
        {
            instance = queue.Dequeue();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(prefab, position, rotation);
            instanceToPrefab[instance] = prefab;
        }

        // Tum child'lardaki IPoolable'lari cagir
        // (EnemyHealth root'ta, EnemyHealthBar child'da olabilir)
        foreach (var poolable in instance.GetComponentsInChildren<IPoolable>(true))
            poolable.OnSpawnFromPool();

        return instance;
    }

    public void Return(GameObject instance)
    {
        if (instance == null) return;

        // Tum child'lardaki IPoolable'lari cagir
        foreach (var poolable in instance.GetComponentsInChildren<IPoolable>(true))
            poolable.OnReturnToPool();

        instance.SetActive(false);

        if (instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools[prefab] = queue;
            }
            queue.Enqueue(instance);
        }
        else
        {
            Destroy(instance);
        }
    }

    public void ReturnDelayed(GameObject instance, float delay)
    {
        StartCoroutine(ReturnAfterDelay(instance, delay));
    }

    private IEnumerator ReturnAfterDelay(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(instance);
    }
}
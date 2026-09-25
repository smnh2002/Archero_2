using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnDirector : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public GameObject bossPrefab;

    [Header("Arena Sinirlari")]
    public Vector2 arenaMin = new Vector2(-10f, -7f);
    public Vector2 arenaMax = new Vector2(8f, 14f);

    [Header("Boss Giris Efekti")]
    [SerializeField] private GameObject bossSpawnEffectPrefab;
    [SerializeField] private float bossEntranceDelay = 1.5f;
    [SerializeField] private float cameraShakeDuration = 0.6f;
    [SerializeField] private float cameraShakeMagnitude = 0.3f;

    [Header("Odul (Ekonomi) Ayarlari")]
    public int baseEnemyGoldReward = 2;
    public int bossGoldReward = 50;

    [Header("Run Suresi")]
    public float runDurationSeconds = 150f;

    [Header("Spawn Sikligi (saniye)")]
    public float initialSpawnInterval = 3f;
    public float finalSpawnInterval = 0.8f;

    [Header("Zorluk Ayarlari (Can & Hasar)")]
    public float mapDamageMultiplier = 1f;

    public float initialHealthMultiplier = 1f;
    public float finalHealthMultiplier = 2.5f;

    [Header("Elite Ayarlari")]
    [Range(0f, 1f)] public float initialEliteChance = 0.05f;
    [Range(0f, 1f)] public float finalEliteChance = 0.25f;
    public float eliteHealthBonus = 2f;
    public float eliteScaleMultiplier = 1.3f;
    public float eliteDamageBonus = 1.5f;

    [Header("Limitler")]
    public int maxConcurrentEnemies = 15;

    [Header("Boss Spawn Ayarlari")]
    [SerializeField] private float bossSpawnDistanceFromPlayer = 5f;

    [Header("Spawn Dogrulama ve Engel Filtresi")]
    [Tooltip("Dusmanin etrafindaki engellerden (kaya, sutun, agac vb.) olmasi gereken minimum temiz yaricap")]
    [SerializeField] private float obstacleClearanceRadius = 1.0f;

    [Tooltip("Dusmanlarin oyuncuya olan minimum spawn mesafesi")]
    [SerializeField] private float minPlayerDistance = 6.0f;

    [Tooltip("Maksimum gecerli zemin Y seviyesi (ustunde asset olan yuksek yerleri engellemek icin)")]
    [SerializeField] private float maxGroundHeight = 0.5f;

    private float runStartTime;
    private float nextSpawnTime;
    private bool bossSpawned = false;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private Transform cachedPlayerTransform;

    private void Start()
    {
        runStartTime = Time.time;
        nextSpawnTime = Time.time + initialSpawnInterval;
    }

    private void Update()
    {
        if (bossSpawned) return;

        float elapsed = Time.time - runStartTime;

        if (elapsed >= runDurationSeconds)
        {
            SpawnBoss();
            bossSpawned = true;
            return;
        }

        float progress = Mathf.Clamp01(elapsed / runDurationSeconds);

        activeEnemies.RemoveAll(e => e == null);

        if (Time.time >= nextSpawnTime && activeEnemies.Count < maxConcurrentEnemies)
        {
            SpawnEnemy(progress);

            float currentInterval = Mathf.Lerp(initialSpawnInterval, finalSpawnInterval, progress);
            nextSpawnTime = Time.time + currentInterval;
        }
    }

    private void SpawnEnemy(float progress)
    {
        if (enemyPrefabs.Count == 0) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Vector3 point = GetRandomSpawnPoint();

        GameObject instance = PoolManager.Instance != null
            ? PoolManager.Instance.Get(prefab, point, Quaternion.identity)
            : Instantiate(prefab, point, Quaternion.identity);

        activeEnemies.Add(instance);

        float healthMultiplier = Mathf.Lerp(initialHealthMultiplier, finalHealthMultiplier, progress);
        float eliteChance = Mathf.Lerp(initialEliteChance, finalEliteChance, progress);
        bool isElite = Random.value < eliteChance;

        EnemyHealth health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.ConfigureForSpawn(healthMultiplier, isElite, eliteHealthBonus, eliteScaleMultiplier, baseEnemyGoldReward);
            health.OnDeath += () => activeEnemies.Remove(instance);
        }

        EnemyCombat combat = instance.GetComponent<EnemyCombat>();
        if (combat != null)
        {
            combat.ConfigureForSpawn(isElite, eliteDamageBonus, mapDamageMultiplier);
        }

        if (isElite)
        {
            Debug.Log($"<color=orange>ELITE dusman spawn oldu:</color> {prefab.name}");
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;

        ClearActiveEnemies();

        Vector3 point = GetBossSpawnPointNearPlayer();
        StartCoroutine(BossEntranceSequence(point));
    }

    private void ClearActiveEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = activeEnemies[i];
            if (enemy != null)
            {
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null && !health.IsDead)
                {
                    health.TakeDamage(new DamageData(99999f, Vector3.zero, 0f, true));
                }
                else
                {
                    Destroy(enemy);
                }
            }
        }
        activeEnemies.Clear();
    }

    private Vector3 GetBossSpawnPointNearPlayer()
    {
        EnsurePlayerCached();

        if (cachedPlayerTransform == null)
        {
            return GetRandomSpawnPoint();
        }

        Vector3 playerPos = cachedPlayerTransform.position;
        float bossClearance = Mathf.Max(obstacleClearanceRadius * 1.8f, 1.8f);

        float baseAngle = Random.Range(0f, 360f);
        for (int i = 0; i < 24; i++)
        {
            float angle = baseAngle + (i * 15f);
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * bossSpawnDistanceFromPlayer;
            Vector3 candidate = playerPos + offset;

            if (TryValidateSpawnPosition(candidate, bossClearance, out Vector3 validPos))
            {
                return validPos;
            }
        }

        return GetRandomSpawnPoint();
    }

    private IEnumerator BossEntranceSequence(Vector3 point)
    {
        Debug.Log("<color=red>BOSS GELIYOR!</color>");

        if (bossSpawnEffectPrefab != null)
        {
            Instantiate(bossSpawnEffectPrefab, point, Quaternion.identity);
        }

        CameraFollow cf = null;
        if (Camera.main != null)
        {
            cf = Camera.main.GetComponent<CameraFollow>();
        }

        if (cf != null)
        {
            cf.Shake(cameraShakeDuration, cameraShakeMagnitude);
        }

        yield return new WaitForSeconds(bossEntranceDelay);

        GameObject boss = Instantiate(bossPrefab, point, Quaternion.identity);
        boss.GetComponent<BossHealth>()?.SetGoldReward(bossGoldReward);
        StartCoroutine(BossRiseAnimation(boss.transform));

        Debug.Log("<color=red>BOSS SPAWN OLDU!</color>");
    }

    private IEnumerator BossRiseAnimation(Transform bossTransform)
    {
        Vector3 targetScale = bossTransform.localScale;
        bossTransform.localScale = Vector3.zero;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            bossTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        bossTransform.localScale = targetScale;
    }

    private Vector3 GetRandomSpawnPoint()
    {
        EnsurePlayerCached();

        // 1. Asama: Oyuncudan uzakta, acik zemin uzerinde nokta ara (50 deneme)
        for (int i = 0; i < 50; i++)
        {
            float x = Random.Range(arenaMin.x, arenaMax.x);
            float z = Random.Range(arenaMin.y, arenaMax.y);
            Vector3 candidate = new Vector3(x, transform.position.y, z);

            if (cachedPlayerTransform != null)
            {
                float dx = x - cachedPlayerTransform.position.x;
                float dz = z - cachedPlayerTransform.position.z;
                if ((dx * dx + dz * dz) < (minPlayerDistance * minPlayerDistance))
                {
                    continue;
                }
            }

            if (TryValidateSpawnPosition(candidate, obstacleClearanceRadius, out Vector3 validPos))
            {
                return validPos;
            }
        }

        // 2. Asama: Oyuncu mesafe kisitini gevsetip tekrar ara (30 deneme)
        for (int i = 0; i < 30; i++)
        {
            float x = Random.Range(arenaMin.x, arenaMax.x);
            float z = Random.Range(arenaMin.y, arenaMax.y);
            Vector3 candidate = new Vector3(x, transform.position.y, z);

            if (TryValidateSpawnPosition(candidate, obstacleClearanceRadius * 0.75f, out Vector3 validPos))
            {
                return validPos;
            }
        }

        // 3. Asama: Son care olarak NavMesh uzerinde zemine en yakin nokta
        Vector3 fallbackCenter = new Vector3((arenaMin.x + arenaMax.x) / 2f, 0f, (arenaMin.y + arenaMax.y) / 2f);
        if (NavMesh.SamplePosition(fallbackCenter, out NavMeshHit hit, 8f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return fallbackCenter;
    }

    private bool TryValidateSpawnPosition(Vector3 candidatePosition, float clearanceRadius, out Vector3 validSpawnPosition)
    {
        validSpawnPosition = Vector3.zero;

        // 1. Yukaridan asagiya Raycast (Asset'in ustune spawn olmayi kesin engeller)
        Ray ray = new Ray(new Vector3(candidatePosition.x, 30f, candidatePosition.z), Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit hit, 45f, ~0, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        // Ilk carptigi nesne KESINLIKLE Ground olmalidir!
        int groundLayer = LayerMask.NameToLayer("Ground");
        bool isGround = (groundLayer >= 0 && hit.collider.gameObject.layer == groundLayer)
                     || hit.collider.CompareTag("Ground")
                     || hit.collider.name.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (!isGround)
        {
            return false;
        }

        if (hit.point.y > maxGroundHeight || hit.point.y < -1.5f)
        {
            return false;
        }

        Vector3 groundPoint = hit.point;

        // 2. Cevresel Engel Kontrolu (Physics.OverlapSphere)
        Collider[] nearby = Physics.OverlapSphere(
            groundPoint + Vector3.up * 0.7f,
            clearanceRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (var col in nearby)
        {
            if ((groundLayer >= 0 && col.gameObject.layer == groundLayer) ||
                col.CompareTag("Ground") ||
                col.name.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                col.CompareTag("Player") ||
                col.CompareTag("Enemy") ||
                col.isTrigger)
            {
                continue;
            }

            return false;
        }

        // 3. NavMesh Kontrolu ve Yukseklik Dogrulamasi
        if (NavMesh.SamplePosition(groundPoint, out NavMeshHit navHit, 1.2f, NavMesh.AllAreas))
        {
            if (Mathf.Abs(navHit.position.y - groundPoint.y) < 0.45f)
            {
                validSpawnPosition = navHit.position;
                return true;
            }
        }

        return false;
    }

    private void EnsurePlayerCached()
    {
        if (cachedPlayerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) cachedPlayerTransform = player.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((arenaMin.x + arenaMax.x) / 2f, transform.position.y, (arenaMin.y + arenaMax.y) / 2f);
        Vector3 size = new Vector3(arenaMax.x - arenaMin.x, 0.1f, arenaMax.y - arenaMin.y);
        Gizmos.DrawWireCube(center, size);
    }
}





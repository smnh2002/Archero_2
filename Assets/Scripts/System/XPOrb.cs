using UnityEngine;

public class XPOrb : MonoBehaviour, IPoolable
{
    [SerializeField] private float xpValue = 5f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float pickupRadius = 3f;

    private Transform player;
    private PlayerExperience playerExperience;
    private bool isCollecting = false;

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerExperience = playerObj.GetComponent<PlayerExperience>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= pickupRadius) isCollecting = true;

        if (isCollecting)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            if (dist < 0.3f) Collect();
        }
    }

    private void Collect()
    {
        playerExperience?.AddXP(xpValue);

        // YENÝ: Destroy yerine havuza dön
        if (PoolManager.Instance != null)
            PoolManager.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }

    // ---- IPoolable ----

    public void OnSpawnFromPool()
    {
        isCollecting = false;
        if (player == null) FindPlayer(); // ilk oluþturulduðunda bulunmuþ olsa da güvenlik için
    }

    public void OnReturnToPool()
    {
        // ekstra temizlik gerekmiyor þu an
    }
}
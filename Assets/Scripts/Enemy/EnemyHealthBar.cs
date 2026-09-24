using UnityEngine;
using MicroBarComponent = Microlight.MicroBar.MicroBar;

/// <summary>
/// Enemy üzerindeki can barını yönetir.
///
/// Billboard (Kameraya bakma) işlemi artık Billboard.cs tarafından yönetiliyor.
/// </summary>
public class EnemyHealthBar : MonoBehaviour, IPoolable
{
    [Header("Health Bar Prefab")]
    [Tooltip("Inspector'dan bir MicroBar prefabi surukle (ornek: Delayed_ImageMicroBar)")]
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Pozisyon Ayarlari")]
    [SerializeField] private Vector3 offset      = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector3 barScale    = new Vector3(0.008f, 0.008f, 0.008f);
    [SerializeField] private float   canvasWidth  = 160f;
    [SerializeField] private float   canvasHeight = 30f;

    private EnemyHealth       enemyHealth;
    private MicroBarComponent microBar;
    private GameObject        barInstance;
    private Canvas            barCanvas;
    private Transform         canvasTransform;
    private bool              barBuilt = false;

    private void Awake()
    {
        // EnemyHealth'i bu obje, parent veya child'larda ara
        enemyHealth = GetComponent<EnemyHealth>()
                   ?? GetComponentInParent<EnemyHealth>()
                   ?? GetComponentInChildren<EnemyHealth>(true);
    }

    private void Start()
    {
        BuildBar();    // Bar'ı oluştur
        BindEvents();  // Event'lere bağlan
        InitBarValue();// Şu anki can değerini yansıt
    }

    private void OnDestroy()
    {
        UnbindEvents();
        if (barInstance != null) Destroy(barInstance);
    }

    // ─── IPoolable ──────────────────────────────────────────────────────────
    public void OnSpawnFromPool()
    {
        if (barInstance != null) barInstance.SetActive(true);
        InitBarValue();
    }

    public void OnReturnToPool()
    {
        if (barInstance != null) barInstance.SetActive(false);
    }

    // ─── Bar oluşturma ──────────────────────────────────────────────────────
    private void BuildBar()
    {
        if (barBuilt) return;

        if (healthBarPrefab == null)
        {
            Debug.LogWarning($"[EnemyHealthBar] {name}: healthBarPrefab atanmamis!");
            return;
        }

        var canvasGo = new GameObject("EnemyHPCanvas");
        canvasGo.transform.SetParent(transform);
        canvasGo.transform.localPosition = offset;
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale    = barScale;
        canvasTransform = canvasGo.transform;

        // Billboard bileşenini ekle (Kameraya bakmasını sağlar)
        canvasGo.AddComponent<Billboard>();

        barCanvas = canvasGo.AddComponent<Canvas>();
        barCanvas.renderMode   = RenderMode.WorldSpace;
        barCanvas.sortingOrder = 10;
        
        var cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam != null) barCanvas.worldCamera = cam;

        var canvasRT = canvasGo.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(canvasWidth, canvasHeight);

        barInstance = Instantiate(healthBarPrefab, canvasGo.transform);
        barInstance.transform.localPosition    = Vector3.zero;
        barInstance.transform.localRotation    = Quaternion.identity;
        barInstance.transform.localScale       = Vector3.one;

        var barRT = barInstance.GetComponent<RectTransform>();
        if (barRT != null)
        {
            barRT.anchorMin        = Vector2.zero;
            barRT.anchorMax        = Vector2.one;
            barRT.offsetMin        = Vector2.zero;
            barRT.offsetMax        = Vector2.zero;
            barRT.localScale       = Vector3.one;
            barRT.localPosition    = Vector3.zero;
            barRT.localEulerAngles = Vector3.zero;
        }

        microBar = barInstance.GetComponent<MicroBarComponent>()
                ?? barInstance.GetComponentInChildren<MicroBarComponent>(true);

        if (microBar == null)
        {
            Debug.LogError($"[EnemyHealthBar] {name}: Prefab'da MicroBar bulunamadi!");
            return;
        }

        barBuilt = true;
    }

    // ─── Event bağlama ──────────────────────────────────────────────────────
    private void BindEvents()
    {
        if (enemyHealth == null) return;
        enemyHealth.OnHealthChanged += HandleHealthChanged;
        enemyHealth.OnDeath        += HandleDeath;
    }

    private void UnbindEvents()
    {
        if (enemyHealth == null) return;
        enemyHealth.OnHealthChanged -= HandleHealthChanged;
        enemyHealth.OnDeath        -= HandleDeath;
    }

    private void InitBarValue()
    {
        if (!barBuilt || microBar == null || enemyHealth == null) return;
        microBar.Initialize(enemyHealth.MaxHealth);
        microBar.UpdateBar(enemyHealth.CurrentHealth);
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (!barBuilt || microBar == null) return;
        if (!Mathf.Approximately(max, microBar.MaxValue))
            microBar.Initialize(max);
        microBar.UpdateBar(current);
    }

    private void HandleDeath()
    {
        if (barInstance != null) barInstance.SetActive(false);
    }
}
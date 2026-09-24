using UnityEngine;
using MicroBarComponent = Microlight.MicroBar.MicroBar;

public class BossHealthBar : MonoBehaviour
{
    [Header("Health Bar Prefab")]
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Pozisyon Ayarlari")]
    [SerializeField] private Vector3 offset      = new Vector3(0f, 3.5f, 0f);
    [SerializeField] private Vector3 barScale    = new Vector3(0.012f, 0.012f, 0.012f);
    [SerializeField] private float   canvasWidth  = 200f;
    [SerializeField] private float   canvasHeight = 36f;

    private BossHealth        bossHealth;
    private MicroBarComponent microBar;
    private GameObject        barInstance;
    private Canvas            barCanvas;
    private Transform         canvasTransform;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>()
                  ?? GetComponentInParent<BossHealth>()
                  ?? GetComponentInChildren<BossHealth>(true);
    }

    private void Start()
    {
        CreateHealthBar();

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += HandleHealthChanged;
            bossHealth.OnDeath         += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= HandleHealthChanged;
            bossHealth.OnDeath         -= HandleDeath;
        }
        if (barInstance != null) Destroy(barInstance);
    }

    private void CreateHealthBar()
    {
        if (healthBarPrefab == null) { Debug.LogWarning($"[BossHealthBar] {name}: prefab atanmamis!"); return; }

        var canvasGo = new GameObject("BossHPCanvas");
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

        if (microBar != null) microBar.Initialize(bossHealth != null ? bossHealth.MaxHealth : 500f);
        else Debug.LogError($"[BossHealthBar] {name}: Prefab'da MicroBar bulunamadi!");
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (microBar == null) return;
        microBar.UpdateBar(current);
    }

    private void HandleDeath()
    {
        if (barInstance != null) barInstance.SetActive(false);
    }
}
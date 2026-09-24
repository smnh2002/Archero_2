using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GameScene'deki HP/Coin/Level text'lerini VE altlarindaki dolgu barlarini yonetir.
/// Player, CurrencyManager ve PlayerExperience'a dogrudan erisir.
/// Her Update frame'inde polling ile degerleri okur — event baglantisina gerek yok.
/// </summary>
public class GameStatsDisplay : MonoBehaviour
{
    public static GameStatsDisplay Instance { get; private set; }

    [Header("Textler (Inspector'dan ata)")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Barlar (Inspector'dan ata — Image / Filled tipinde)")]
    [Tooltip("Level / XP bari — bas boş, dusmanlar oldurdukce dolar")]
    [SerializeField] private Image xpBarFill;
    [Tooltip("Coin bari — bas boş, coin toplandikca dolar")]
    [SerializeField] private Image coinBarFill;
    [Tooltip("Health bari — bas dolu, hasar alindikca azalir")]
    [SerializeField] private Image healthBarFill;

    [Header("Coin Bar Ayari")]
    [Tooltip("Kac coin'de bar tamamen dolar")]
    [SerializeField] private int coinMax = 500;

    // Cachelenmis referanslar
    private PlayerHealth     cachedHealth;
    private PlayerExperience cachedExp;

    // Player arama zamanlayicisi
    private float findTimer = 0f;
    private const float FIND_INTERVAL = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Barlari baslangic degerlerine ayarla
        if (healthBarFill != null) healthBarFill.fillAmount = 1f; // Health: dolu
        if (xpBarFill     != null) xpBarFill.fillAmount     = 0f; // XP: bos
        if (coinBarFill   != null) coinBarFill.fillAmount    = 0f; // Coin: bos

        // Textleri sifirla
        if (statsText  != null) statsText.text  = "Level: 1";
        if (coinText   != null) coinText.text   = "Coin: 0";
        if (healthText != null) healthText.text = "HP: --/--";

        // Hemen bir kez referans bul
        FindReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─── Ana guncelleme dongusu ──────────────────────────────────────────────
    private void Update()
    {
        // Referanslar eksikse periodically ara
        if (cachedHealth == null || cachedExp == null)
        {
            findTimer -= Time.deltaTime;
            if (findTimer <= 0f)
            {
                findTimer = FIND_INTERVAL;
                FindReferences();
            }
        }

        UpdateHealthUI();
        UpdateXPUI();
        UpdateCoinUI();
    }

    // ─── Referans bulma ──────────────────────────────────────────────────────
    private void FindReferences()
    {
        if (cachedHealth == null || cachedExp == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (cachedHealth == null)
                    cachedHealth = player.GetComponent<PlayerHealth>() ?? player.GetComponentInChildren<PlayerHealth>();
                if (cachedExp == null)
                    cachedExp = player.GetComponent<PlayerExperience>() ?? player.GetComponentInChildren<PlayerExperience>();
            }
        }

        if (cachedExp == null)
            cachedExp = PlayerExperience.Instance;
    }

    // ─── UI guncelleme metodlari ─────────────────────────────────────────────

    private void UpdateHealthUI()
    {
        if (cachedHealth == null) return;

        float current = cachedHealth.CurrentHealth;
        float max     = cachedHealth.MaxHealth;

        // Text
        if (healthText != null)
            healthText.text = $"HP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

        // Bar: 1.0 = dolu (baslangic), azalikca kuculur
        if (healthBarFill != null)
            healthBarFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 1f;
    }

    private void UpdateXPUI()
    {
        if (cachedExp == null) return;

        int   level   = cachedExp.CurrentLevel;
        float xp      = cachedExp.CurrentXP;
        float xpToNext = cachedExp.XPToNextLevel;

        // Text
        if (statsText != null)
            statsText.text = $"Level: {level}";

        // Bar: 0.0 = bos (baslangic), dusmanlar oldurdukce dolar, level atlayinca sifirlanir
        if (xpBarFill != null)
            xpBarFill.fillAmount = xpToNext > 0f ? Mathf.Clamp01(xp / xpToNext) : 0f;
    }

    private void UpdateCoinUI()
    {
        if (CurrencyManager.Instance == null) return;

        int gold = CurrencyManager.Instance.CurrentGold;

        // Text
        if (coinText != null)
            coinText.text = $"Coin: {gold}";

        // Bar: 0.0 = bos (baslangic), coin toplandikca dolar
        if (coinBarFill != null)
            coinBarFill.fillAmount = Mathf.Clamp01((float)gold / coinMax);
    }

    // ─── Dis erisim (PlayerExperience LevelUp'ta cagirir) ───────────────────
    public void UpdateLevel(int newLevel)
    {
        if (statsText != null)
            statsText.text = $"Level: {newLevel}";
    }
}
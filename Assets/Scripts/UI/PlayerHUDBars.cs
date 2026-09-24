using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Tm sahnelerde Player barini cizen sistem.
/// Artik GameStatsDisplay olmayan sahnelerde HP, Coin ve Level yazlarn da gosterir.
/// </summary>
public class PlayerHUDBars : MonoBehaviour
{
    private static PlayerHUDBars _instance;

    private static readonly string[] excludedScenes =
        { "MenuScene", "MenuScene", "Menu", "SampleScene" };

    private GameObject canvasGo;
    private Image healthFill;
    private Image coinFill;
    private Image xpFill;

    // YENI: Dinamik olusturulan yazilar
    private GameObject textsContainer;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI levelText;

    private PlayerHealth cachedHealth;
    private PlayerExperience cachedExp;

    private float findTimer = 0f;
    private const float FIND_INTERVAL = 0.5f;
    private const int COIN_MAX = 500;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;
        var go = new GameObject("[PlayerHUDBars]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PlayerHUDBars>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BuildCanvas();
        RefreshVisibility(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (canvasGo != null) Destroy(canvasGo);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cachedHealth = null;
        cachedExp = null;
        findTimer = 0f;
        RefreshVisibility(scene.name);
    }

    private void Update()
    {
        if (canvasGo == null || !canvasGo.activeSelf) return;

        if (cachedHealth == null || cachedExp == null)
        {
            findTimer -= Time.deltaTime;
            if (findTimer <= 0f)
            {
                findTimer = FIND_INTERVAL;
                FindReferences();
            }
        }

        // --- Bars ---
        if (healthFill != null && cachedHealth != null)
            healthFill.fillAmount = cachedHealth.MaxHealth > 0f ? Mathf.Clamp01(cachedHealth.CurrentHealth / cachedHealth.MaxHealth) : 1f;

        if (xpFill != null && cachedExp != null)
            xpFill.fillAmount = cachedExp.XPToNextLevel > 0f ? Mathf.Clamp01(cachedExp.CurrentXP / cachedExp.XPToNextLevel) : 0f;

        if (coinFill != null && CurrencyManager.Instance != null)
            coinFill.fillAmount = Mathf.Clamp01((float)CurrencyManager.Instance.CurrentGold / COIN_MAX);

        // --- Texts ---
        if (textsContainer != null && textsContainer.activeSelf)
        {
            if (healthText != null && cachedHealth != null)
                healthText.text = $"HP: {Mathf.CeilToInt(cachedHealth.CurrentHealth)}/{Mathf.CeilToInt(cachedHealth.MaxHealth)}";

            if (levelText != null && cachedExp != null)
                levelText.text = $"Level: {cachedExp.CurrentLevel}";

            if (coinText != null && CurrencyManager.Instance != null)
                coinText.text = $"Coin: {CurrencyManager.Instance.CurrentGold}";
        }
    }

    private void RefreshVisibility(string sceneName)
    {
        if (canvasGo == null) return;
        
        bool isExcluded = false;
        foreach (var s in excludedScenes)
        {
            if (s == sceneName) { isExcluded = true; break; }
        }
        
        canvasGo.SetActive(!isExcluded);

        // GameScene'de kendi UI'si oldugu icin yazilari gizle, diger maplerde goster
        if (textsContainer != null)
        {
            bool hasOwnHUD = FindObjectOfType<GameStatsDisplay>() != null;
            textsContainer.SetActive(!hasOwnHUD);
        }
    }

    private void FindReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (cachedHealth == null) cachedHealth = player.GetComponent<PlayerHealth>();
            if (cachedExp == null) cachedExp = player.GetComponent<PlayerExperience>() ?? player.GetComponentInChildren<PlayerExperience>();
        }
        if (cachedExp == null) cachedExp = PlayerExperience.Instance;
    }

    private void BuildCanvas()
    {
        canvasGo = new GameObject("PlayerHUDBars_Canvas");
        DontDestroyOnLoad(canvasGo);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9;

        var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        scaler.referencePixelsPerUnit = 100f;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Barlar (Eski konumlar korunuyor)
        const float W = 210f;
        const float H = 16f;
        const float dropY = 33f;

        healthFill = MakeBar("HUD_Health", new Vector2(-812f, 303f - dropY), new Vector2(W, H), new Color(0.1f, 0.85f, 0.2f), true);
        coinFill = MakeBar("HUD_Coin", new Vector2(-813f, 388f - dropY), new Vector2(W, H), new Color(1f, 0.85f, 0f), false);
        xpFill = MakeBar("HUD_XP", new Vector2(-813f, 473f - dropY), new Vector2(W, H), new Color(0.9f, 0.15f, 0.15f), false);

        // --- Yazilar ---
        textsContainer = new GameObject("TextsContainer");
        textsContainer.transform.SetParent(canvasGo.transform, false);

        // GameScene'deki orijinal konum, hizalama ve boyutlara gore uretiliyor:
        healthText = MakeText("HealthText", "HP: --/--", new Vector2(-774.12f, 303f), new Vector2(275.76f, 50f));
        coinText = MakeText("CoinText", "Coin: 0", new Vector2(-813f, 388.1f), new Vector2(200f, 50f));
        levelText = MakeText("LevelText", "Level: 1", new Vector2(-809f, 469f), new Vector2(200f, 50f));
        
        // Eger GameScene'den kopya bir font bulabilirsek onu atayalim (oyun baslangicinda GameScene ise)
        var existingDisplay = FindObjectOfType<GameStatsDisplay>();
        if (existingDisplay != null)
        {
            var field = typeof(GameStatsDisplay).GetField("statsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var tmp = field.GetValue(existingDisplay) as TextMeshProUGUI;
                if (tmp != null && tmp.font != null)
                {
                    healthText.font = tmp.font;
                    coinText.font = tmp.font;
                    levelText.font = tmp.font;
                }
            }
        }
    }

    private TextMeshProUGUI MakeText(string name, string defText, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(textsContainer.transform, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defText;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        // GameScene'deki orijinal hizalama merkezi baz alinarak ayarlandi
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        return tmp;
    }

    private Sprite whiteSprite;

    private Image MakeBar(string id, Vector2 pos, Vector2 size, Color color, bool startFull)
    {
        if (whiteSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        var bg = new GameObject(id + "_BG");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.sprite = whiteSprite;
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.5f, 0.5f);
        bgRT.anchorMax = new Vector2(0.5f, 0.5f);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = size;
        bgRT.anchoredPosition = pos;

        var fill = new GameObject(id + "_Fill");
        fill.transform.SetParent(bg.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.sprite = whiteSprite;
        fillImg.color = color;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = startFull ? 1f : 0f;
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        return fillImg;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ses Kaynaklari")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Muzik Klipleri (BGM)")]
    public AudioClip menuMusicClip;
    public AudioClip gameplayMusicClip;

    [Header("Ses Klipleri (SFX)")]
    public AudioClip playerHitClip;
    public AudioClip enemyHitClip;
    public AudioClip playerDeathClip;
    public AudioClip levelUpClip;
    public AudioClip buttonClickClip;
    public AudioClip victoryClip;

    private float lastButtonClickTime = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource kaynaklarini garantiye al
        AudioSource[] sources = GetComponents<AudioSource>();
        if (musicSource == null)
        {
            musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        }
        if (sfxSource == null || sfxSource == musicSource)
        {
            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        }

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Ses ayarini Save dosyasindan cek
        if (SaveManager.Instance != null)
        {
            SetVolume(SaveManager.Instance.SaveData.masterVolume);
        }

        // Oyun basladiginda mevcut sahneye uygun muzigi baslat
        UpdateMusicForScene(SceneManager.GetActiveScene().name);

        // Sahnedeki butonlara ses bagla
        HookAllButtons();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
        HookAllButtons();
    }

    public void UpdateMusicForScene(string sceneName)
    {
        if (sceneName == "MenuScene" || sceneName.ToLower().Contains("menu"))
        {
            PlayMusic(menuMusicClip);
        }
        else
        {
            // Diger sahneler (GameScene, NewMapScene, NewMap2Scene, NewMap3Scene vb.)
            PlayMusic(gameplayMusicClip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        // Eger calinmak istenen muzik zaten caliyorsa bastan baslatma, kesintisiz devam etsin
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null) return;
        // Ayni anda cok ses calarsa kesilmemesi icin PlayOneShot kullaniyoruz
        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    public void PlayButtonClick()
    {
        // Hizli tiklamalarda veya cift tetiklemelerde ust uste binmeyi onlemek icin debounce
        if (Time.unscaledTime - lastButtonClickTime < 0.05f) return;
        lastButtonClickTime = Time.unscaledTime;

        PlaySFX(buttonClickClip);
    }

    /// <summary>
    /// Sahnedeki tum butonlari (inaktif paneller dahil) bulur ve tiklama sesini otomatik baglar.
    /// </summary>
    public void HookAllButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            btn.onClick.RemoveListener(PlayButtonClick);
            btn.onClick.AddListener(PlayButtonClick);
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveData.masterVolume = volume;
            SaveManager.Instance.SaveGame();
        }
    }
}

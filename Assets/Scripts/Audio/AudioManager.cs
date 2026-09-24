using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ses Kaynaklarý")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Ses Klipleri (SFX)")]
    public AudioClip playerHitClip;
    public AudioClip enemyHitClip;
    public AudioClip playerDeathClip;
    public AudioClip levelUpClip;
    public AudioClip buttonClickClip;
    public AudioClip victoryClip;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Ses ayarýný Save dosyasýndan çek
        if (SaveManager.Instance != null)
        {
            SetVolume(SaveManager.Instance.SaveData.masterVolume);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        // Ayný anda çok ses çalarsa kesilmemesi için PlayOneShot kullanýyoruz
        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    public void PlayButtonClick() => PlaySFX(buttonClickClip);

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
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ResetForNewRun()
    {
        // PlayerHealth ve PlayerController oyun sahnesindedir;
        // bunlara singleton veya tag araciligiyla erisiyoruz.
        var stats = PlayerStats.Instance;           // DontDestroyOnLoad singleton
        PlayerHealth health = null;
        PlayerExperience experience = null;
        PlayerController controller = null;

        // Ayni GameObject'te mi? (bazı proje kurulumlarinda olabilir)
        if (stats != null)
        {
            health = stats.GetComponent<PlayerHealth>();
            experience = stats.GetComponent<PlayerExperience>();
            controller = stats.GetComponent<PlayerController>();
        }

        // Yoksa sahneden tag ile bul
        if (health == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                health = player.GetComponent<PlayerHealth>();
                experience = player.GetComponent<PlayerExperience>();
                controller = player.GetComponent<PlayerController>();
            }
        }

        // 1. Olum/fizik durumunu temizle
        health?.ResetDeathState();

        // 2. Stat'lari yeniden hesapla (base + meta bonuslar)
        //    Bu islem icinde SyncMaxHealthFromStats() de cagirilir.
        stats?.ResetStats();

        // 3. Eger health ayni GameObject'te degilse, elle senkronize et
        if (health != null && (stats == null || stats.GetComponent<PlayerHealth>() == null))
        {
            health.SyncMaxHealthFromStats();
        }

        // 4. Level/XP sifirla
        experience?.ResetProgress();

        // 5. Controller'i aktif et
        controller?.ResetController();

        Debug.Log($"<color=lime>=== YENI RUN BASLADI === MaxHealth: {health?.MaxHealth} | CurrentHealth: {health?.CurrentHealth}</color>");
    }
}

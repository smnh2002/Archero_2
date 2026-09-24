using UnityEngine;
using UnityEngine.SceneManagement; // YENÝ: Sahne geçiþi için bu kütüphaneyi ekledik

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    // YENÝ: Inspector'dan da deðiþtirebilmen için sahne adýný buraya ekledik
    [SerializeField] private string nextSceneName = "NewMapScene";

    private void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(newState == GameState.Victory); if (newState == GameState.Victory) { if (PlayerPrefs.GetInt("MaxUnlockedLevel", 1) < 2) { PlayerPrefs.SetInt("MaxUnlockedLevel", 2); PlayerPrefs.Save(); } }
    }

    public void OnContinueButtonPressed()
    {
        // GameManager'ýn hafýzasýndaki eski ismi kullanmak yerine direkt kendi ismimizi yüklüyoruz:
        SceneManager.LoadScene(nextSceneName);

        // Sahne yüklendiðinde oyunun donuk kalmamasý (zamanýn akmasý) için state'i güncelliyoruz:
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
    }
}

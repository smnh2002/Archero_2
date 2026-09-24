using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMapVictoryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Sahne Ayarlari")]
    [SerializeField] private string nextSceneName = "NewMap2Scene";

    // ------------------------------------------------------------------ //
    private void Start()
    {
        // Panel baslangicta kapali olmali
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    // ------------------------------------------------------------------ //
    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        if (victoryPanel == null) return;

        if (newState == GameState.Victory)
        {
            victoryPanel.SetActive(true); if (PlayerPrefs.GetInt("MaxUnlockedLevel", 1) < 3) { PlayerPrefs.SetInt("MaxUnlockedLevel", 3); PlayerPrefs.Save(); }
            victoryPanel.SetActive(true); if (PlayerPrefs.GetInt("MaxUnlockedLevel", 1) < 4) { PlayerPrefs.SetInt("MaxUnlockedLevel", 4); PlayerPrefs.Save(); }
        }
        else
        {
            victoryPanel.SetActive(false);
        }
    }

    // ------------------------------------------------------------------ //
    /// <summary>
    /// Inspector'daki butonuna bagla.
    /// </summary>
    public void OnBackButtonPressed()
    {
        // Zaman donuk kalmamasi icin timeScale'i sifirla
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);

        SceneManager.LoadScene(nextSceneName);
    }
}

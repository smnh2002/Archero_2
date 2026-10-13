using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Ogeleri")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        // GameManager statini guvenceye al
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.MainMenu);

        // Istatistikleri Yukle
        if (SaveManager.Instance != null)
        {
            var data = SaveManager.Instance.SaveData;
            if (statsText != null)
            {
                statsText.text = $"Highest Level: {data.highestLevelReached}\nRuns Completed: {data.completedRuns}";
            }
            if (volumeSlider != null)
            {
                volumeSlider.value = data.masterVolume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        // Sahnedeki tum butonlara (paneller dahil) tiklama sesini otomatik bagla
        AudioManager.Instance?.HookAllButtons();
    }

    public void StartGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene(gameSceneName);

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log("Oyundan cikildi");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SetVolume(float value)
    {
        AudioManager.Instance?.SetVolume(value);
    }
}

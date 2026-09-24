using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI ��eleri")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private string gameSceneName = "GameScene"; // YEN� - eksikti

    private void Start()
    {
        // GameManager statini g�venceye al
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.MainMenu);

        // �statistikleri Y�kle
        if (SaveManager.Instance != null)
        {
            var data = SaveManager.Instance.SaveData;
            // YEN�: Yaz� objesi sahnede varsa yazd�r, yoksa hata verme ge�!
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
    }

    public void StartGame()
    {
        
        AudioManager.Instance?.PlayButtonClick();
        
        // PlayerPersistence.Instance?.ResetForNewRun();

     
        SceneManager.LoadScene(gameSceneName);

        
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);

    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log("Oyundan ��k�ld�");
        Application.Quit();

        // 2. Unity Edit�r� i�indeyken "Play" modundan ��kmam�z� sa�lar
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif


    }

    public void SetVolume(float value)
    {
        AudioManager.Instance?.SetVolume(value);
    }
}

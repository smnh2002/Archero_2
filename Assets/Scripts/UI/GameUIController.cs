using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullan�yorsan

public class GameUIController : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject levelUpPanel;

    [Header("Level Up UI ��eleri")]
    [SerializeField] private Button[] upgradeButtons; // 3 adet buton s�r�kle
    [SerializeField] private TextMeshProUGUI[] upgradeNames; // Butonlar�n i�indeki yaz�lar

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        // Panelleri ba�lang��ta kapat
        pausePanel.SetActive(false);
        defeatPanel.SetActive(false);
        levelUpPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        pausePanel.SetActive(newState == GameState.Paused);
        defeatPanel.SetActive(newState == GameState.Defeat);
        levelUpPanel.SetActive(newState == GameState.LevelUp);

        if (newState == GameState.LevelUp)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.levelUpClip);
            SetupLevelUpButtons();
        }
        else if (newState == GameState.Defeat)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.playerDeathClip);
            // �l�nce skoru kaydet (PlayerExperience referans�na ihtiyac�n olabilir)
            SaveManager.Instance?.UpdateRunStats(PlayerExperience.Instance != null ? PlayerExperience.Instance.CurrentLevel : 1, false);
        }
    }

    private void SetupLevelUpButtons()
    {
        var choices = UpgradeManager.Instance.CurrentChoices;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeNames[i].text = choices[i].upgradeName; // UpgradeData i�indeki isme g�re ayarla

                // Butona t�klan�nca ne olaca��n� tan�mla (�nceki eventleri temizle)
                upgradeButtons[i].onClick.RemoveAllListeners();
                int index = i; // Closure sorunu ya�amamak i�in yerel de�i�kene al
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeButtonClicked(index));
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnUpgradeButtonClicked(int index)
    {
        AudioManager.Instance?.PlayButtonClick();
        UpgradeManager.Instance.ChooseUpgrade(index); // GameManager statini 'Playing'e senin yazd���n bu metod �ekiyor zaten.
    }

    // --- BUTON METOTLARI (Inspector'dan butonlara ba�la) ---

    public void ResumeGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance.ResumeGame();
    }

    public void RestartGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameManager.Instance.SetState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f; // Men�ye d�nerken zaman� d�zeltmeyi unutma
        SceneManager.LoadScene("MenuScene"); // Kendi men� sahnene g�re ismini de�i�tir
        GameManager.Instance.SetState(GameState.MainMenu);
    }
}

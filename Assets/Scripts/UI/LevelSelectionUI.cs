using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelsPanel;

    [Header("Level Buttons")]
    [SerializeField] private Button btnLevel1;
    [SerializeField] private Button btnLevel2;
    [SerializeField] private Button btnLevel3;
    [SerializeField] private Button btnLevel4;

    private void Start()
    {
        if (levelsPanel != null) levelsPanel.SetActive(false);
        RefreshLocks();

        if (btnLevel1 != null) btnLevel1.onClick.AddListener(() => LoadLevel("GameScene"));
        if (btnLevel2 != null) btnLevel2.onClick.AddListener(() => LoadLevel("NewMapScene"));
        if (btnLevel3 != null) btnLevel3.onClick.AddListener(() => LoadLevel("NewMap2Scene"));
        if (btnLevel4 != null) btnLevel4.onClick.AddListener(() => LoadLevel("NewMap3Scene"));
    }

    public void OpenLevelsPanel()
    {
        // Don't hide mainMenuPanel so its background remains visible!
        if (levelsPanel != null) levelsPanel.SetActive(true);
        RefreshLocks();
    }

    public void CloseLevelsPanel()
    {
        if (levelsPanel != null) levelsPanel.SetActive(false);
    }

    private void RefreshLocks()
    {
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        
        if (btnLevel1 != null) btnLevel1.interactable = true;
        if (btnLevel2 != null) btnLevel2.interactable = (maxUnlocked >= 2);
        if (btnLevel3 != null) btnLevel3.interactable = (maxUnlocked >= 3);
        if (btnLevel4 != null) btnLevel4.interactable = (maxUnlocked >= 4);
    }

    private void LoadLevel(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
        SceneManager.LoadScene(sceneName);
    }
}

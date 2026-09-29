using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    LevelUp,
    Paused,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public event System.Action<GameState, GameState> OnStateChanged;

    [Header("Sahne Gecisi")]
    [SerializeField] private string nextSceneName = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState oldState = CurrentState;
        CurrentState = newState;

        Debug.Log($"<color=cyan>Game State Degisti:</color> {oldState} -> {newState}");

        HandleTimeScale(newState);
        OnStateChanged?.Invoke(oldState, newState);
    }

    private void HandleTimeScale(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            default:
                Time.timeScale = 0f;
                break;
        }
    }

    public void StartRun() => SetState(GameState.Playing);
    public void PauseGame() { if (CurrentState == GameState.Playing) SetState(GameState.Paused); }
    public void ResumeGame() { if (CurrentState == GameState.Paused) SetState(GameState.Playing); }
    public void TriggerLevelUp() => SetState(GameState.LevelUp);
    
    public void TriggerVictory()
    {
        SetState(GameState.Victory);
        UnlockNextLevel();
    }

    private void UnlockNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentLevelIndex = 1;

        if (currentScene == "GameScene") currentLevelIndex = 1;
        else if (currentScene == "NewMapScene") currentLevelIndex = 2;
        else if (currentScene == "NewMap2Scene") currentLevelIndex = 3;
        else if (currentScene == "NewMap3Scene") currentLevelIndex = 4;

        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (maxUnlocked <= currentLevelIndex)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", currentLevelIndex + 1);
            PlayerPrefs.Save();
            Debug.Log($"<color=green>Level {currentLevelIndex + 1} unlocked!</color>");
        }
    }

    public void TriggerDefeat() => SetState(GameState.Defeat);

    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("GameManager: Next Scene Name atanmadi! Inspector'dan doldur.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
        SetState(GameState.Playing);
    }

    public void LoadSceneByName(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Sahne adi bos olamaz!");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
        SetState(GameState.Playing);
    }
}

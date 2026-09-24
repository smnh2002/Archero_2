using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState  // YENÝ - eksik olan buydu
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

    [Header("Sahne Geçiþi")]
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

        Debug.Log($"<color=cyan>Game State Deðiþti:</color> {oldState} -> {newState}");

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
    public void TriggerVictory() => SetState(GameState.Victory);
    public void TriggerDefeat() => SetState(GameState.Defeat);

    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("GameManager: Next Scene Name atanmamýþ! Inspector'dan doldur.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
        SetState(GameState.Playing);
    }

    // Bu yeni metod, içine yazacaðýmýz sahne adýný direkt yükleyecek
    public void LoadSceneByName(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Sahne adý boþ olamaz!");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
        SetState(GameState.Playing); // Zamanýn (Time.timeScale) tekrar 1 olmasýný garantiye alýyoruz
    }

}
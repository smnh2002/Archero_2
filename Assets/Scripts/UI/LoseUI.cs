using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseUI : MonoBehaviour
{
    private static LoseUI _activeInstance;

    [SerializeField] private GameObject losePanel;
    [SerializeField] private string menuSceneName = "MenuScene";

    private void Awake()
    {
        // Sahnede zaten bir LoseUI varsa bu duplicate'i yok et
        if (_activeInstance != null && _activeInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        _activeInstance = this;
    }

    private void Start()
    {
        if (losePanel != null) losePanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (_activeInstance == this) _activeInstance = null;
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        if (losePanel != null)
            losePanel.SetActive(newState == GameState.Defeat);
    }

    public void OnBackButtonPressed()
    {
        if (losePanel != null) losePanel.SetActive(false);
        SceneManager.LoadScene(menuSceneName);

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.MainMenu);
    }
}
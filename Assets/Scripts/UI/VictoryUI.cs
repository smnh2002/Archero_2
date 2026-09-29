using UnityEngine;
using UnityEngine.SceneManagement; 

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
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
            victoryPanel.SetActive(newState == GameState.Victory); 
    }

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene(nextSceneName);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
    }
}

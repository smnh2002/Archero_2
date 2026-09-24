using UnityEngine;
using System;

public class PlayerExperience : MonoBehaviour
{
    // YEN�: Singleton Instance eklendi
    public static PlayerExperience Instance { get; private set; }

    [SerializeField] private int startingXPToNextLevel = 10;
    [SerializeField] private float xpCurveMultiplier = 1.25f;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXP { get; private set; } = 0f;
    public float XPToNextLevel { get; private set; }

    public event Action<float, float> OnXPChanged;
    public event Action<int> OnLevelUp;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Awake()
    {
        // Ok i�areti yerine s�sl� paranteze ge�tik ve Instance'� tan�mlad�k
        Instance = this;
        XPToNextLevel = startingXPToNextLevel;
    }


    // YEN�: Yeni run ba�larken �a�r�lacak
    public void ResetProgress()
    {
        CurrentLevel = 1;
        CurrentXP = 0f;
        XPToNextLevel = startingXPToNextLevel;

        if (GameStatsDisplay.Instance != null)
        {
            GameStatsDisplay.Instance.UpdateLevel(CurrentLevel);
        }

        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }

    public void AddXP(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            Debug.Log(GameManager.Instance.CurrentState);
            return;
        }

        CurrentXP += amount;
        Debug.Log(CurrentXP);

        while (CurrentXP >= XPToNextLevel)
        {
            CurrentXP -= XPToNextLevel;
            LevelUp();
        }

        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }

    private void LevelUp()
    {
        CurrentLevel++;
        XPToNextLevel = Mathf.Round(XPToNextLevel * xpCurveMultiplier);

        Debug.Log($"<color=cyan>LEVEL UP!</color> Yeni Level: {CurrentLevel}");

        // YEN�: UI'daki yaz�m�z� karakterin g�ncel leveliyle g�ncelliyoruz!
        if (GameStatsDisplay.Instance != null)
        {
            GameStatsDisplay.Instance.UpdateLevel(CurrentLevel);
        }

        OnLevelUp?.Invoke(CurrentLevel);
        GameManager.Instance?.TriggerLevelUp();
    }
}

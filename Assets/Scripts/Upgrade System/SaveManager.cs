using UnityEngine;
using System.IO;

[System.Serializable]
public class GameSaveData
{
    public float masterVolume = 1f;
    public int highestLevelReached = 1;
    public int totalKills = 0;
    public int completedRuns = 0;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public GameSaveData SaveData { get; private set; }
    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Application.persistentDataPath + "/gamesave.json";
        LoadGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(SaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"<color=green>Oyun Kaydedildi!</color> Yol: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData = JsonUtility.FromJson<GameSaveData>(json);
        }
        else
        {
            SaveData = new GameSaveData(); // Ýlk defa açýlýyorsa sýfýr data yarat
        }
    }

    // Oyun bittiðinde çaðýracaðýz (Victory veya Defeat)
    public void UpdateRunStats(int reachedLevel, bool isVictory)
    {
        if (reachedLevel > SaveData.highestLevelReached)
            SaveData.highestLevelReached = reachedLevel;

        if (isVictory)
            SaveData.completedRuns++;

        SaveGame();
    }
}
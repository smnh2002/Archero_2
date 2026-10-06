using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem; // BÝZE BU LAZIM!

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    [SerializeField] private int choicesPerLevelUp = 3;

    public List<UpgradeData> CurrentChoices { get; private set; } = new List<UpgradeData>();
    public event System.Action<List<UpgradeData>> OnUpgradeChoicesReady; // UI ileride buna abone olacak

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.LevelUp) PresentChoices();
    }

    private void PresentChoices()
    {
        CurrentChoices = allUpgrades.OrderBy(u => Random.value).Take(choicesPerLevelUp).ToList();

        if (CurrentChoices.Count > 0)
        {
            ChooseUpgrade(Random.Range(0, CurrentChoices.Count));
        }
        else
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
    }

    public void ChooseUpgrade(int index)
    {
        Debug.Log($"<color=cyan>ChooseUpgrade çaðrýldý. Index: {index}, CurrentChoices count: {CurrentChoices.Count}, State: {GameManager.Instance.CurrentState}</color>"); // GEÇÝCÝ

        if (GameManager.Instance.CurrentState != GameState.LevelUp) return;
        if (index < 0 || index >= CurrentChoices.Count) return;

        ApplyUpgrade(CurrentChoices[index]);
        GameManager.Instance.SetState(GameState.Playing);
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        var stats = PlayerStats.Instance;
        if (stats == null) return;

        switch (upgrade.effectType)
        {
            case UpgradeEffectType.StatModifier:
                stats.Stats.AddModifier(new StatModifier(upgrade.statType, upgrade.modifierType, upgrade.value, upgrade.upgradeId));
                Debug.Log($"<color=green>Upgrade uygulandý:</color> {upgrade.upgradeName}");
                break;

            case UpgradeEffectType.InstantHeal:
                stats.GetComponent<PlayerHealth>()?.Heal(upgrade.healAmount);
                break;
        }
    }
}
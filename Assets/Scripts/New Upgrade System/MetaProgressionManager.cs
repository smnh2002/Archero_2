using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    [SerializeField] private List<MetaUpgradeData> allMetaUpgrades = new List<MetaUpgradeData>();

    private Dictionary<string, int> levels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("<color=red>MetaProgressionManager: Fazladan instance yok edildi.</color>"); // GEÇÝCÝ
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("<color=lime>MetaProgressionManager: Yeni Instance oluþturuldu, level'lar sýfýrlandý.</color>"); // GEÇÝCÝ

        // Artýk PlayerPrefs'ten okumuyoruz, hepsi 0'dan baþlýyor
        foreach (var upgrade in allMetaUpgrades)
        {
            levels[upgrade.upgradeId] = 0;
        }
    }

    public int GetLevel(string upgradeId)
    {
        return levels.TryGetValue(upgradeId, out int lvl) ? lvl : 0;
    }

    public int GetCost(MetaUpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade.upgradeId);
        return Mathf.RoundToInt(upgrade.baseCost * Mathf.Pow(upgrade.costMultiplierPerLevel, currentLevel));
    }

    public bool IsMaxLevel(MetaUpgradeData upgrade)
    {
        return GetLevel(upgrade.upgradeId) >= upgrade.maxLevel;
    }

    public bool TryUpgrade(MetaUpgradeData upgrade)
    {
        if (IsMaxLevel(upgrade)) return false;

        int cost = GetCost(upgrade);
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpendGold(cost)) return false;

        levels[upgrade.upgradeId] = GetLevel(upgrade.upgradeId) + 1;
        // PlayerPrefs.SetInt satýrý kaldýrýldý - artýk kaydedilmiyor

        return true;
    }

    public void ApplyMetaBonuses(StatSheet stats)
    {
        foreach (var upgrade in allMetaUpgrades)
        {
            int level = GetLevel(upgrade.upgradeId);

          

            if (level <= 0) continue;

            float totalValue = upgrade.valuePerLevel * level;
            stats.AddModifier(new StatModifier(upgrade.statType, upgrade.modifierType, totalValue, "meta_" + upgrade.upgradeId));

            Debug.Log($"<color=magenta>Uygulanan bonus: {upgrade.upgradeId} -> +{totalValue}</color>"); // GEÇÝCÝ
        }
    }
}
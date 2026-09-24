using UnityEngine;

[CreateAssetMenu(fileName = "NewMetaUpgrade", menuName = "Game/Meta Upgrade")]
public class MetaUpgradeData : ScriptableObject
{
    public string upgradeId;
    public string upgradeName;
    [TextArea] public string description;

    public StatType statType;
    public ModifierType modifierType;
    public float valuePerLevel = 0.05f; // her level baþýna eklenecek miktar

    public int maxLevel = 10;
    public int baseCost = 50;
    [Range(1.05f, 2f)] public float costMultiplierPerLevel = 1.15f; // her levelde maliyet %15 artar
}
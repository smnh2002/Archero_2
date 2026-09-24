using UnityEngine;

public enum UpgradeEffectType { StatModifier, InstantHeal }

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeId;
    public string upgradeName;
    [TextArea] public string description;

    public UpgradeEffectType effectType = UpgradeEffectType.StatModifier;

    [Header("Stat Modifier ise")]
    public StatType statType;
    public ModifierType modifierType;
    public float value;

    [Header("Instant Heal ise")]
    public float healAmount;
}
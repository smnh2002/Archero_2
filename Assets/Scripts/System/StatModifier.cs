using UnityEngine;

public enum ModifierType { Flat, PercentAdditive, PercentMultiplicative }

[System.Serializable]
public struct StatModifier
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;
    public string source; // hangi upgrade'den geldiði - kaldýrma/stack takibi için

    public StatModifier(StatType statType, ModifierType modifierType, float value, string source)
    {
        this.statType = statType;
        this.modifierType = modifierType;
        this.value = value;
        this.source = source;
    }
}
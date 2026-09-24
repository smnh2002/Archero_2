using UnityEngine;

using System.Collections.Generic;
using System.Linq;

public class StatSheet
{
    private readonly Dictionary<StatType, float> baseValues = new Dictionary<StatType, float>();
    private readonly List<StatModifier> modifiers = new List<StatModifier>();

    public void SetBaseValue(StatType type, float value) => baseValues[type] = value;

    public float GetBaseValue(StatType type) => baseValues.TryGetValue(type, out float v) ? v : 0f;

    public void AddModifier(StatModifier modifier) => modifiers.Add(modifier);

    public bool RemoveModifiersFromSource(string source) => modifiers.RemoveAll(m => m.source == source) > 0;

    // Hesaplama sýrasý: (base + flat) * (1 + %additive toplamý) * çarpýmsal(1 + %mult)
    public float GetValue(StatType type)
    {
        float baseVal = GetBaseValue(type);

        float flatSum = modifiers.Where(m => m.statType == type && m.modifierType == ModifierType.Flat).Sum(m => m.value);
        float percentAddSum = modifiers.Where(m => m.statType == type && m.modifierType == ModifierType.PercentAdditive).Sum(m => m.value);

        float result = (baseVal + flatSum) * (1f + percentAddSum);

        foreach (var m in modifiers.Where(m => m.statType == type && m.modifierType == ModifierType.PercentMultiplicative))
            result *= (1f + m.value);

        return result;
    }
}
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Degerler")]
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseCooldownReduction = 0f;
    [SerializeField] private float baseAreaSize = 1f;
    [SerializeField] private float baseProjectileCount = 1f;

    public StatSheet Stats { get; private set; }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        InitializeStats();
    }

    private void InitializeStats()
    {
        Stats = new StatSheet();

        Stats.SetBaseValue(StatType.Damage, baseDamage);
        Stats.SetBaseValue(StatType.MoveSpeed, baseMoveSpeed);
        Stats.SetBaseValue(StatType.AttackSpeed, baseAttackSpeed);
        Stats.SetBaseValue(StatType.MaxHealth, baseMaxHealth);
        Stats.SetBaseValue(StatType.CooldownReduction, baseCooldownReduction);
        Stats.SetBaseValue(StatType.AreaSize, baseAreaSize);
        Stats.SetBaseValue(StatType.ProjectileCount, baseProjectileCount);

        // Meta bonusları uygula (can upgrade'i dahil)
        MetaProgressionManager.Instance?.ApplyMetaBonuses(Stats);

        // Eğer PlayerHealth sahnede varsa MaxHealth'i senkronize et
        GetComponent<PlayerHealth>()?.SyncMaxHealthFromStats();
    }

    /// <summary>
    /// Yeni run başlarken çağrılır: run-içi upgrade'leri temizler, meta bonusları korur.
    /// </summary>
    public void ResetStats()
    {
        InitializeStats();
    }

    public float Damage => Stats.GetValue(StatType.Damage);
    public float MoveSpeed => Stats.GetValue(StatType.MoveSpeed);
    public float AttackSpeed => Stats.GetValue(StatType.AttackSpeed);
    public float MaxHealth => Stats.GetValue(StatType.MaxHealth);
    public float CooldownReduction => Stats.GetValue(StatType.CooldownReduction);
    public float AreaSize => Stats.GetValue(StatType.AreaSize);
    public int ProjectileCount => Mathf.RoundToInt(Stats.GetValue(StatType.ProjectileCount));
}

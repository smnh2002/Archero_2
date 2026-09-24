using UnityEngine;

/// <summary>
/// Hasar alabilen her þey bu interface'i implement eder.
/// Player, Enemy, Boss, hatta kýrýlabilir objeler bile.
/// </summary>
public interface IDamageable
{
    void TakeDamage(DamageData damage);
}
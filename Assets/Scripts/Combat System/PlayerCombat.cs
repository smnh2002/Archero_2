using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    public Transform attackPoint; // Vuruþun merkez noktasý (Örn: karakterin eli veya kýlýcýn ucu)
    public float attackRange = 1.5f; // Vuruþun etki alaný yarýçapý
    public float attackDamage = 25f; // Verilecek hasar miktarý
    public float knockbackForce = 5f; // Ýttirme gücü
    public LayerMask enemyLayers; // Sadece bu layer'daki objelere hasar ver (Enemy layer'ý seçmelisin)

    [Header("Kalkan Saldýrýsý Ayarlarý")]
    public Transform shieldAttackPoint; // kalkanýn olduðu noktaya yakýn bir empty
    public float shieldAttackRange = 1.8f;
    public float shieldAttackDamage = 10f; // normal saldýrýdan daha az hasar
    public float shieldKnockbackForce = 15f; // ama daha güçlü itme


    // Bu metodu ShieldBash animasyon klibine Animation Event olarak ekleyeceksin
    private void DealShieldDamage()
    {
        if (shieldAttackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(shieldAttackPoint.position, shieldAttackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageData damage = new DamageData
                {
                    amount = PlayerStats.Instance != null ? PlayerStats.Instance.Damage * 0.4f : shieldAttackDamage,
                    sourcePosition = transform.position,
                    knockbackForce = shieldKnockbackForce
                };
                damageable.TakeDamage(damage);

                VFXManager.Instance?.PlayBloodEffect(enemy.ClosestPoint(shieldAttackPoint.position));
            }
        }
    }

    // Bu metodu Unity üzerinden Player'ýn Attack animasyonuna Animation Event olarak ekleyeceðiz.
    private void DealDamage()
    {
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageData damage = new DamageData
                {
                    amount = PlayerStats.Instance != null ? PlayerStats.Instance.Damage : attackDamage,
                    sourcePosition = transform.position,
                    knockbackForce = knockbackForce
                };
                damageable.TakeDamage(damage);

                // YENÝ: kan efekti
                VFXManager.Instance?.PlayBloodEffect(enemy.ClosestPoint(attackPoint.position));
            }
        }
    }

    // Unity Editor'de vuruþ alanýný (kýrmýzý top þeklinde) görmek için
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
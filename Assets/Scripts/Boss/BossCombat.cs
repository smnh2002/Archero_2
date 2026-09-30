using UnityEngine;

public class BossCombat : MonoBehaviour
{
    [Header("Normal Saldiri (AttackNormal - Tek Bicak)")]
    public Transform attackPointRight;
    public float normalAttackRange = 1.2f;
    public float normalAttackDamage = 20f;
    public float normalKnockback = 10f;
    public float normalAttackCooldown = 2.5f;

    [Header("Ozel Saldiri (AttackSpecial - Iki Bicak)")]
    public Transform attackPointLeft;
    public float specialAttackRange = 1.2f;
    public float specialAttackDamage = 35f;
    public float specialKnockback = 20f;
    public float specialAttackCooldown = 6f;

    [Header("Saldiri Secim Ayarlari")]
    [Range(0f, 1f)] public float specialAttackChance = 0.5f;

    public LayerMask playerLayer;

    private Transform player;
    private Animator anim;
    private BossHealth bossHealth;
    private BossAI bossAI;

    private float nextNormalAttackTime = 0f;
    private float nextSpecialAttackTime = 0f;
    private bool isAttacking = false;
    private float attackStartTime = 0f; // SAFETY TIMEOUT
    public bool IsAttacking => isAttacking;

    private void Start()
    {
        anim = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        bossAI = GetComponent<BossAI>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        // SAFETY TIMEOUT: Eger boss bir sekilde animasyonda takili kalirsa saldiriyi iptal et (3 saniye timeout)
        if (isAttacking && Time.time - attackStartTime > 3f)
        {
            isAttacking = false;
        }

        if (player == null || isAttacking) return;
        if (bossHealth != null && bossHealth.IsDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float effectiveRange = bossAI != null ? bossAI.saldiriMesafesi : normalAttackRange;

        if (distanceToPlayer > effectiveRange) return;

        bool normalReady = Time.time >= nextNormalAttackTime;
        bool specialReady = Time.time >= nextSpecialAttackTime;

        if (!normalReady && !specialReady) return;

        bool useSpecial;
        if (normalReady && specialReady)
            useSpecial = Random.value < specialAttackChance;
        else
            useSpecial = specialReady;

        if (useSpecial)
        {
            isAttacking = true;
            attackStartTime = Time.time;
            anim.ResetTrigger("AttackNormal");
            anim.SetTrigger("AttackSpecial");
            nextSpecialAttackTime = Time.time + specialAttackCooldown;
        }
        else
        {
            isAttacking = true;
            attackStartTime = Time.time;
            anim.ResetTrigger("AttackSpecial");
            anim.SetTrigger("AttackNormal");
            nextNormalAttackTime = Time.time + normalAttackCooldown;
        }
    }

    /// <summary>
    /// BossHealth tarafindan cagrilir: hasar aninda isAttacking'i sifirlar
    /// boylece boss bir sonraki Update'te hemen saldirabilir.
    /// </summary>
    public void ForceResetAttacking()
    {
        isAttacking = false;
        // Boss vurulduğunda girdiği "Hit" animasyonu sırasında, BossCombat'ın hemen yeni bir 
        // saldırı tetikleyip animasyonu (ve isAttacking değişkenini) bozmasını engellemek için 
        // kısa bir süreliğine saldırıları duraklatıyoruz.
        float hitStunDuration = 0.6f;
        nextNormalAttackTime = Mathf.Max(nextNormalAttackTime, Time.time + hitStunDuration);
        nextSpecialAttackTime = Mathf.Max(nextSpecialAttackTime, Time.time + hitStunDuration);
    }

    // Animation Event - Attack01 klibinin vurus anina ekle
    public void DealNormalDamage()
    {
        DealDamageAt(attackPointRight, normalAttackRange, normalAttackDamage, normalKnockback);
    }

    // Animation Event - Attack02 klibinin vurus anina ekle
    public void DealSpecialDamage()
    {
        DealDamageAt(attackPointRight, specialAttackRange, specialAttackDamage, specialKnockback);
        DealDamageAt(attackPointLeft, specialAttackRange, specialAttackDamage, specialKnockback);
    }

    // Animation Event - her iki klibin SONUNA ekle
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    private void DealDamageAt(Transform point, float range, float damageAmount, float knockback)
    {
        if (point == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(point.position, range, playerLayer);
        foreach (Collider hitPlayer in hitPlayers)
        {
            IDamageable damageable = hitPlayer.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageData damage = new DamageData(damageAmount, transform.position, knockback, false);
                damageable.TakeDamage(damage);
                VFXManager.Instance?.PlayBloodEffect(hitPlayer.ClosestPoint(point.position));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPointRight != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPointRight.position, normalAttackRange);
        }
        if (attackPointLeft != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPointLeft.position, specialAttackRange);
        }
    }
}
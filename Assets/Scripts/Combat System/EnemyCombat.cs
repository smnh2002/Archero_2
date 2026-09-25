using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    [Tooltip("Düþmanýn yumruk attýðý eli. Ýçine boþ bir obje açýp buraya sürükle.")]
    public Transform attackPoint;
    public float attackRange = 1.2f;
    public float attackDamage = 15f;
    public float knockbackForce = 15f;
    public LayerMask playerLayer;

    [Header("AI & Animasyon Ayarlarý")]
    private Transform player; // Hedef alýnacak oyuncu
    public float attackCooldown = 2f; // Ýki saldýrý arasýndaki bekleme süresi
    private float nextAttackTime = 0f;
    private Animator anim; // Animator referansý
                           
    // EnemyCombat.cs içine ekle
    private float baseAttackDamage;

    private void Start()
    {
        // Karakterin üzerindeki Animator bileþenini alýyoruz
        anim = GetComponent<Animator>();

        // Eðer player baþtan atanmadýysa, sahnede "Player" tag'i ile bulmaya çalýþ
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Oyuncu ile düþman arasýndaki mesafeyi ölç
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Eðer oyuncu saldýrý menzilindeyse
        if (distanceToPlayer <= attackRange)
        {
            // Ve saldýrý bekleme süresi (cooldown) dolduysa
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // Oyuncu menzilden çýkarsa isAttacking'i false yap (Blend Tree'ye dönmesi için)
            anim.SetBool("isAttacking", false);
        }
    }

    private void Awake() // eðer Awake yoksa ekle, varsa içine bu satýrý koy
    {
        baseAttackDamage = attackDamage;
    }

    // YENÝ: SpawnDirector tarafýndan çaðrýlacak
    public void ConfigureForSpawn(bool isElite, float eliteDamageBonus, float mapDamageMultiplier)
    {
        float dmg = baseAttackDamage * mapDamageMultiplier;
        attackDamage = isElite ? dmg * eliteDamageBonus : dmg;
    }

    private void Attack()
    {
        // Ekran görüntüsündeki "isAttacking" bool parametresini tetikle
        anim.SetBool("isAttacking", true);

        // ÝPUCU: Eðer Attack01 ve Attack02 arasýnda rastgele seçim yapmak istersen
        // Ekran görüntüsündeki "AttackID" parametresini burada kullanabilirsin:
        // int randomAttack = Random.Range(0, 2); 
        // anim.SetFloat("AttackID", randomAttack); 
    }

    // Bu metodu animasyonda yumruðun tam hedefe vardýðý kareye ekleyeceksin (Animation Event)
    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        foreach (Collider hitPlayer in hitPlayers)
        {
            IDamageable damageable = hitPlayer.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageData damage = new DamageData(attackDamage, transform.position, knockbackForce, false);
                damageable.TakeDamage(damage);

                // YENÝ: kan efekti
                VFXManager.Instance?.PlayBloodEffect(hitPlayer.ClosestPoint(attackPoint.position));
            }
        }


    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable, IPoolable
{
    [Header("XP")]
    [SerializeField] private GameObject xpOrbPrefab;

    [Header("Can Ayarlarý")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Görsel Feedback")]
    [SerializeField] private Renderer[] renderersToFlash;

    [Header("Hit Flash Ayarlarý")]
    [SerializeField] private Material flashMaterial; // Inspector'dan FlashWhite'ý sürükle
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Ödül")]
    [SerializeField] private int goldReward = 2; // YENÝ

    private Material[][] originalMaterials; // her renderer'ýn orijinal materyalleri
    private float baseMaxHealth; // YENÝ: pool'dan tekrar tekrar kullanýlýrken referans deðer
    private Vector3 baseScale;   // YENÝ: elite scale sýfýrlamasý için

    private float currentHealth;
    private bool isDead = false;

    private Animator animator;
    private NavMeshAgent agent;
    private EnemyAI enemyAI;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();

        baseMaxHealth = maxHealth;
        baseScale = transform.localScale;
        currentHealth = maxHealth;

        // YENÝ: Orijinal materyalleri önbelleðe al
        if (renderersToFlash != null)
        {
            originalMaterials = new Material[renderersToFlash.Length][];
            for (int i = 0; i < renderersToFlash.Length; i++)
            {
                if (renderersToFlash[i] != null)
                    originalMaterials[i] = renderersToFlash[i].materials;
            }
        }
    }

    // YENÝ: SpawnDirector tarafýndan her spawn'da çaðrýlacak, base deðerden hesaplar (compounding olmaz)
    public void ConfigureForSpawn(float healthMultiplier, bool isElite, float eliteHealthBonus, float eliteScaleMultiplier, int newGoldReward)
    {
        float finalMultiplier = isElite ? healthMultiplier * eliteHealthBonus : healthMultiplier;

        maxHealth = baseMaxHealth * finalMultiplier;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        transform.localScale = isElite ? baseScale * eliteScaleMultiplier : baseScale;

        goldReward = newGoldReward;
    }

    public void TakeDamage(DamageData damage)
    {
        if (isDead) return;

        currentHealth -= damage.amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.playerHitClip);

        // Hasar yazisini goster (Floating Damage Text)
        DamagePopupManager.Create(transform.position, damage.amount);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        // YENÝ: Hasar alma animasyonu (ölmediyse tetiklenir, Die ile çakýþmasýn diye üstteki return'den sonra)
        if (animator != null)
        {
            animator.ResetTrigger("Hit"); // YENÝ - bekleyen/kuyruktaki Hit trigger'ýný temizle
            animator.SetTrigger("Hit");
        }

        if (damage.knockbackForce > 0f)
        {
            StartCoroutine(ApplyKnockback(damage.sourcePosition, damage.knockbackForce));
        }
        StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator ApplyKnockback(Vector3 sourcePosition, float force)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            Vector3 direction = (transform.position - sourcePosition).normalized;
            direction.y = 0f;
            agent.velocity = direction * force;
            yield return new WaitForSeconds(0.15f);
            if (!isDead && agent.isOnNavMesh)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = false;
            }
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (renderersToFlash == null || renderersToFlash.Length == 0 || flashMaterial == null) yield break;

        // Tüm renderer'larý beyaz materyale çevir
        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            if (renderersToFlash[i] == null) continue;

            Material[] flashMats = new Material[renderersToFlash[i].materials.Length];
            for (int j = 0; j < flashMats.Length; j++)
            {
                flashMats[j] = flashMaterial;
            }
            renderersToFlash[i].materials = flashMats;
        }

        yield return new WaitForSeconds(flashDuration);

        // Orijinal materyallere geri dön
        RestoreOriginalMaterials();
    }

    private void RestoreOriginalMaterials()
    {
        if (renderersToFlash == null || originalMaterials == null) return;

        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            if (renderersToFlash[i] != null && originalMaterials[i] != null)
            {
                renderersToFlash[i].materials = originalMaterials[i];
            }
        }
    }

    private void SetRenderersEnabled(bool state)
    {
        foreach (var r in renderersToFlash)
            if (r != null) r.enabled = state;
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        CurrencyManager.Instance?.AddGold(goldReward); // YENÝ

        // XP Orb da artýk pool'dan geliyor
        if (xpOrbPrefab != null)
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.Get(xpOrbPrefab, transform.position, Quaternion.identity);
            else
                Instantiate(xpOrbPrefab, transform.position, Quaternion.identity); // pool yoksa eski yönteme düþ
        }

        // YENÝ: Die tetiklenmeden önce isAttacking'i false yap, Attack transition'ý Die'ýn önüne geçmesin
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.ResetTrigger("Hit"); // YENÝ - bekleyen/kuyruktaki Hit trigger'ýný temizle
            animator.SetTrigger("Die");
        }

        if (animator != null) animator.SetTrigger("Die");
        if (enemyAI != null) enemyAI.enabled = false;
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // YENÝ: Destroy yerine havuza geri dönüþ (3 saniye sonra, ölüm animasyonu izlensin diye)
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnDelayed(gameObject, 3f);
        else
            Destroy(gameObject, 3f);
    }

    // ---- IPoolable ----

    public void OnSpawnFromPool()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (animator != null) animator.Rebind();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        SetRenderersEnabled(true);

        // YENÝ: Agent'ý önce aktif et, sonra pozisyona "warp" et (NavMesh'e saðlam yerleþtir)
        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position); // agent'ý mevcut pozisyona saðlam þekilde baðla
            }
        }

        if (enemyAI != null) enemyAI.enabled = true;
    }

    public void OnReturnToPool()
    {
        StopAllCoroutines(); // knockback/flash coroutine'leri yarýda kalmýþsa durdur
        RestoreOriginalMaterials(); // YENÝ - materyal beyazda takýlý kalmasýn
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Can Ayarlari")]
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float phase2Threshold = 0.5f;

    [Header("Gorsel Feedback")]
    [SerializeField] private Renderer[] renderersToFlash;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.1f;

    private Material[][] originalMaterials;

    private float currentHealth;
    private bool isDead = false;
    private bool isPhase2 = false;

    private Animator animator;
    private NavMeshAgent agent;
    private BossAI bossAI;
    private BossCombat bossCombat;
    private float lastHitAnimTime = -999f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnPhase2Start;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsPhase2 => isPhase2;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        bossAI = GetComponent<BossAI>();
        bossCombat = GetComponent<BossCombat>();
        currentHealth = maxHealth;

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

        // Faz 2 gecisi
        if (!isPhase2 && currentHealth <= maxHealth * phase2Threshold)
        {
            isPhase2 = true;
            OnPhase2Start?.Invoke();
            Debug.Log("<color=orange>BOSS FAZ 2'YE GECTI!</color>");
                }
        
        // Sadece boss saldirmiyorsa (hareket veya idle halindeyse) ve ust uste stun-lock olmamasi icin 1 saniyede bir Hit animasyonu oynatilir.
        if (animator != null) { if (bossCombat == null || !bossCombat.IsAttacking) { if (Time.time - lastHitAnimTime >= 1.5f) { animator.SetTrigger("Hit"); lastHitAnimTime = Time.time; if (bossCombat != null) bossCombat.ForceResetAttacking(); } } }

        StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (renderersToFlash == null || renderersToFlash.Length == 0 || flashMaterial == null) yield break;

        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            if (renderersToFlash[i] == null) continue;
            Material[] flashMats = new Material[renderersToFlash[i].materials.Length];
            for (int j = 0; j < flashMats.Length; j++)
                flashMats[j] = flashMaterial;
            renderersToFlash[i].materials = flashMats;
        }

        yield return new WaitForSeconds(flashDuration);
        RestoreOriginalMaterials();
    }

    private void RestoreOriginalMaterials()
    {
        if (renderersToFlash == null || originalMaterials == null) return;
        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            if (renderersToFlash[i] != null && originalMaterials[i] != null)
                renderersToFlash[i].materials = originalMaterials[i];
        }
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (animator != null) animator.SetTrigger("Die");
        if (bossAI != null) bossAI.enabled = false;
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerVictory();

        Destroy(gameObject, 5f);
    }
}


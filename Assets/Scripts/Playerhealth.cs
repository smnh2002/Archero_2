using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Can Ayarlari")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 0.8f;

    [Header("Knockback Ayarlari")]
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Gorsel Feedback (opsiyonel)")]
    [SerializeField] private Renderer[] renderersToFlash;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.1f;
    private System.Collections.Generic.Dictionary<Renderer, Material[]> originalMaterials;

    [Header("Blok Ayarlari")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float blockDamageReduction = 0.8f;

    private Rigidbody rb;
    private Animator anim;
    [SerializeField] private float currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnKnockbackStart;
    public event Action OnKnockbackEnd;
    public event Action OnDeath;
    public event Action OnDamaged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        // PlayerStats Awake sirasindan bagimsiz olarak maxHealth'i guvenceye al.
        // SyncMaxHealthFromStats() PlayerStats.Awake() tarafindan veya ResetForNewRun() tarafindan cagrilacak.
        maxHealth = (PlayerStats.Instance != null) ? PlayerStats.Instance.MaxHealth : maxHealth;
        currentHealth = maxHealth;

        if (renderersToFlash != null)
        {
            originalMaterials = new System.Collections.Generic.Dictionary<Renderer, Material[]>();
            foreach (var r in renderersToFlash)
            {
                if (r != null)
                    originalMaterials[r] = r.materials;
            }
        }
    }

    public void TakeDamage(DamageData damage)
    {
        if (isDead || isInvulnerable) return;

        if (playerController != null && playerController.IsBlocking)
        {
            damage.amount *= (1f - blockDamageReduction);
            damage.knockbackForce = 0f;
            Debug.Log($"<color=blue>Blok yapildi! Azaltilmis hasar: {damage.amount}</color>");
        }

        currentHealth -= damage.amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"<color=red>Oyuncu Hasar Aldi!</color> Verilen Hasar: {damage.amount} | Kalan Can: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.playerHitClip);

        if (anim != null)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
        }

        if (currentHealth <= 0f)
        {
            Debug.Log("Oyuncu OLDU!");
            Die();
            return;
        }

        if (damage.knockbackForce > 0f)
        {
            StartCoroutine(ApplyKnockback(damage.sourcePosition, damage.knockbackForce));
        }

        StartCoroutine(InvulnerabilityRoutine());
    }

    private void RestoreOriginalMaterials()
    {
        if (renderersToFlash == null || originalMaterials == null) return;
        foreach (var r in renderersToFlash)
        {
            if (r != null && originalMaterials.ContainsKey(r))
                r.materials = originalMaterials[r];
        }
    }

    private IEnumerator ApplyKnockback(Vector3 sourcePosition, float force)
    {
        OnKnockbackStart?.Invoke();

        Vector3 direction = (transform.position - sourcePosition);
        direction.y = 0f;
        direction.Normalize();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        OnKnockbackEnd?.Invoke();
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        const float blinkInterval = 0.15f;
        bool isFlashed = false;

        while (elapsed < invulnerabilityDuration)
        {
            isFlashed = !isFlashed;
            
            if (isFlashed && flashMaterial != null)
            {
                foreach (var r in renderersToFlash)
                {
                    if (r == null) continue;
                    Material[] flashMats = new Material[r.materials.Length];
                    for (int j = 0; j < flashMats.Length; j++) flashMats[j] = flashMaterial;
                    r.materials = flashMats;
                }
            }
            else
            {
                RestoreOriginalMaterials();
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        RestoreOriginalMaterials();
        isInvulnerable = false;
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Yeni run baslarken "diriltme" islemi: can'i tam doldurur.
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        isInvulnerable = false;

        if (PlayerStats.Instance != null) maxHealth = PlayerStats.Instance.MaxHealth;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Sadece olum durumunu/fizigi sifirlar, can hesaplamasina DOKUNMAZ.
    /// </summary>
    public void ResetDeathState()
    {
        isDead = false;
        isInvulnerable = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    /// <summary>
    /// PlayerStats'tan MaxHealth degerini ceker, currentHealth'i tam doldurur
    /// ve UI'ya OnHealthChanged event'i gonderir.
    /// </summary>
    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SyncMaxHealthFromStats()
    {
        if (PlayerStats.Instance == null) return;

        float newMax = PlayerStats.Instance.MaxHealth;
        maxHealth = newMax;
        currentHealth = maxHealth; // Her sync'te tam dolu basla

        Debug.Log($"<color=yellow>SyncMaxHealthFromStats BITTI. Instance ID: {GetInstanceID()}, maxHealth={maxHealth}, currentHealth={currentHealth}</color>");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

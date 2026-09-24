using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // derece/saniye

    [Header("Referanslar")]
    [SerializeField] private Animator animator;

    [Header("Stun Ayarlarý")]
    private bool isStunned = false;
    public bool IsStunned => isStunned;

    [Header("Saldýrý Combo Ayarlarý")]
    [SerializeField] private float comboResetTime = 0.6f; // bu süre içinde tekrar týklanmazsa combo sýfýrlanýr
    private int attackStep = 0; // 0 = idle, 1 = Chop, 2 = Horizontal
    private float lastAttackTime = -10f;

    [Header("Blok Ayarlarý")]
    [SerializeField] private float blockDamageReduction = 0.8f; // %80 hasar azaltma
    private bool isBlocking = false;
    public bool IsBlocking => isBlocking; // PlayerHealth bunu okuyacak


    private Rigidbody rb;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    // Knockback sýrasýnda oyuncu inputu geçici olarak kapatýlýr
    private bool isKnockedBack = false;

    // Step 6'da StatSystem baðlanýnca buradan çekilecek. Þimdilik sabit deðer.
    public float CurrentMoveSpeed => PlayerStats.Instance != null ? PlayerStats.Instance.MoveSpeed : baseMoveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerHealth = GetComponent<PlayerHealth>();

        // Rigidbody'nin X, Y ve Z eksenlerinde fizik motoru tarafýndan döndürülmesini tamamen engelliyoruz
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    private void OnEnable()
    {
        playerHealth.OnKnockbackStart += HandleKnockbackStart;
        playerHealth.OnKnockbackEnd += HandleKnockbackEnd;
        playerHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        playerHealth.OnKnockbackStart -= HandleKnockbackStart;
        playerHealth.OnKnockbackEnd -= HandleKnockbackEnd;
        playerHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        // Karakter ölü veya knockback yiyorsa girdi okumayý durdur
        if (isKnockedBack || isStunned || !enabled) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        ReadBlockInput(); // YENÝ - hareket okumadan önce kontrol edelim
        ReadInput();
        ReadAttackInput();
    }

    private void ReadBlockInput() // YENÝ
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed) // Q basýlý tutulunca blok
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    if (animator != null) animator.SetBool("IsBlocking", true);
                }
            }
            else if (isBlocking)
            {
                isBlocking = false;
                if (animator != null) animator.SetBool("IsBlocking", false);
            }
        }
    }
    private void FixedUpdate()
    {
        if (isKnockedBack || isStunned) return; // knockback sýrasýnda hareket etme, fizik yönetsin

        ApplyMovement();
        ApplyRotation();
        UpdateAnimator();
    }

    private void ReadInput()
    {
        if (isBlocking) // YENÝ - blok sýrasýnda hareket edilemez
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
        }
        moveInput = new Vector2(x, z).normalized;
    }

    // YENÝ EKLENEN METOT: Saldýrý tuþlarýný kontrol eder
    private void ReadAttackInput()
    {
        if (isBlocking) return;

        bool isAttackPressed = false;
        bool isShieldAttackPressed = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isAttackPressed = true;
        }
        else if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isAttackPressed = true;
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            isShieldAttackPressed = true;
        }

        if (isAttackPressed)
        {
            PerformAttack();
        }

        if (isShieldAttackPressed)
        {
            PerformShieldAttack();
        }
    }

    private void PerformShieldAttack() // YENÝ
    {
        if (animator != null)
        {
            animator.SetTrigger("ShieldAttack");
        }
    }

    // YENÝ EKLENEN METOT: Animatörü tetikler
    private void PerformAttack()
    {
        if (animator == null) return;

        float now = Time.time;

        // Combo penceresi kapandýysa (uzun süre týklanmadýysa) baþtan baþla
        if (attackStep == 0 || now - lastAttackTime > comboResetTime)
        {
            attackStep = 1; // Chop
        }
        else if (attackStep == 1)
        {
            attackStep = 2; // Horizontal
        }
        else
        {
            attackStep = 1; // 2'den sonra tekrar baþa dön
        }

        animator.SetInteger("Attack", attackStep);
        lastAttackTime = now;
    }

    private void ApplyMovement()
    {
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        Vector3 targetVelocity = moveDirection * CurrentMoveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // yerçekimini bozma

        rb.linearVelocity = targetVelocity;
    }

    private void ApplyRotation()
    {
        if (moveDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    public void ApplyStun(float duration)
    {
        StopCoroutine(nameof(StunRoutine)); // üst üste stun gelirse süreyi sýfýrdan baþlat
        StartCoroutine(StunRoutine(duration));
    }

    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        moveInput = Vector2.zero;

        if (animator != null) animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        float speed01 = moveDirection.magnitude; // 0-1 arasý, blend tree için
        animator.SetFloat("Speed", speed01);
    }

    private void HandleKnockbackStart()
    {
        isKnockedBack = true;
        moveInput = Vector2.zero;
    }

    private void HandleKnockbackEnd()
    {
        isKnockedBack = false;
    }

    private void HandleDeath()
    {
        enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (animator != null) animator.SetTrigger("Death");

        // YENÝ
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerDefeat();
    }

    // YENÝ: Yeni run baþlarken çaðrýlacak
    public void ResetController()
    {
        enabled = true;
        isKnockedBack = false;
        isBlocking = false;
        isStunned = false;

        // YENÝ: Animator'ý Death state'inden çýkar, normale döndür
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

    }

    // YENÝ: Her saldýrý klibinin SONUNA Animation Event olarak eklenecek
    // Bu olmadan Attack deðeri hiç sýfýrlanmaz, karakter pozda takýlý kalýr
    // (Has Exit Time kapalý olduðu için animasyon otomatik bitmiyor)
    public void ResetAttack()
    {
        if (animator != null)
        {
            animator.SetInteger("Attack", 0);
        }
        attackStep = 0;
    }

}
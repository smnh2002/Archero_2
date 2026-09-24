using UnityEngine;

public class WizardCombat : MonoBehaviour
{
    [Header("Büyü Ayarlarý")]
    public float spellRange = 8f; // büyünün gerçek etki menzili (EnemyAI'daki saldiriMesafesi ile ayný tutulmalý)
    public float spellDamage = 10f;
    public float stunDuration = 1.5f;

    private Transform player;
    private EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // Bu metodu büyü animasyon klibine Animation Event olarak ekleyeceksin
    // (büyünün "gerçekleþtiði" ana, örneðin animasyonun %80-90'ýna denk gelen kareye)
    public void CastSpell()
    {
        if (player == null) return;

        float effectiveRange = enemyAI != null ? enemyAI.saldiriMesafesi : spellRange;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Player o an menzil dýþýna kaçtýysa, büyü boþa gider
        if (distanceToPlayer > effectiveRange)
        {
            Debug.Log("<color=gray>Büyü ýskaladý, oyuncu menzil dýþýna kaçtý.</color>");
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            DamageData damage = new DamageData(spellDamage, transform.position, 0f, false); // knockback yok, sadece stun
            playerHealth.TakeDamage(damage);

            VFXManager.Instance?.PlayBloodEffect(player.position);
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ApplyStun(stunDuration);
            Debug.Log("<color=magenta>Oyuncu büyü ile donduruldu!</color>");
        }
    }
}
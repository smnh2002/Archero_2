using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("Hedef ve Mesafeler")]
    private Transform player;
    public float gorusMesafesi = 15f;
    public float saldiriMesafesi = 2.5f;

    [Header("Faz 2 Ayarlarý")]
    [SerializeField] private float phase2SpeedMultiplier = 1.3f; // faz 2'de biraz hýzlansýn

    private NavMeshAgent agent;
    private Animator animator;
    private PlayerHealth targetHealth;
    private BossHealth bossHealth;
    private float baseAgentSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        baseAgentSpeed = agent.speed;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if (player != null)
        {
            targetHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (bossHealth != null) bossHealth.OnPhase2Start += HandlePhase2Start;
    }

    private void OnDisable()
    {
        if (bossHealth != null) bossHealth.OnPhase2Start -= HandlePhase2Start;
    }

    private void HandlePhase2Start()
    {
        agent.speed = baseAgentSpeed * phase2SpeedMultiplier;
        if (animator != null) animator.SetBool("Phase2", true);
    }

    void Update()
    {
        if (targetHealth != null && targetHealth.IsDead)
        {
            agent.SetDestination(transform.position);
            animator.SetFloat("Speed 2", 0f);
            return;
        }

        float mesafe = Vector3.Distance(transform.position, player.position);
        animator.SetFloat("Speed 2", agent.velocity.magnitude);

        // KOVALAMA
        if (mesafe <= gorusMesafesi && mesafe > saldiriMesafesi)
        {
            agent.SetDestination(player.position);
        }
        // SALDIRI MENZÝLÝNDE (dönüþ burada, tetikleme BossCombat'ta)
        else if (mesafe <= saldiriMesafesi)
        {
            agent.SetDestination(transform.position);
            Vector3 yon = (player.position - transform.position).normalized;
            Quaternion bakisAcisi = Quaternion.LookRotation(new Vector3(yon.x, 0, yon.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, bakisAcisi, Time.deltaTime * 5f);
        }
        // BEKLEME
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gorusMesafesi);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, saldiriMesafesi);
    }
}
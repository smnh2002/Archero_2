using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Hedef ve Mesafeler")]
    private Transform player;
    public float gorusMesafesi = 999f;
    public float saldiriMesafesi = 2f;

    private NavMeshAgent agent;
    private Animator animator;

    // YENÝ: Oyuncunun can sistemini takip etmek için
    private PlayerHealth targetHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // YENÝ: Baþlangýçta oyuncunun saðlýk bileþenini bul ve referans al
        if (player != null)
        {
            targetHealth = player.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        // YENÝ: Agent henüz NavMesh üzerine yerleþmemiþse (pooling'den yeni çýktýysa) hiçbir þey yapma
        if (agent == null || !agent.isOnNavMesh || !agent.enabled) return;

        // YENÝ: Eðer oyuncu varsa ve öldüyse, düþmaný olduðu yerde durdur ve Update'i kes
        if (targetHealth != null && targetHealth.IsDead)
        {
            agent.SetDestination(transform.position); // Dur
            animator.SetBool("isAttacking", false);   // Saldýrmayý býrak
            animator.SetFloat("Speed 2", 0f);         // Yürüme/Koþma animasyonunu sýfýrla
            return; // return diyerek aþaðýdaki kovalama/saldýrma kodlarýnýn çalýþmasýný engelliyoruz
        }

        // ESKÝ KODLARIN AYNI ÞEKÝLDE DEVAM EDÝYOR:
        float mesafe = Vector3.Distance(transform.position, player.position);

        animator.SetFloat("Speed 2", agent.velocity.magnitude);

        // KOVALAMA DURUMU
        if (mesafe <= gorusMesafesi && mesafe > saldiriMesafesi)
        {
            agent.SetDestination(player.position);
            animator.SetBool("isAttacking", false);
        }
        // SALDIRI DURUMU
        else if (mesafe <= saldiriMesafesi)
        {
            agent.SetDestination(transform.position);

            Vector3 yon = (player.position - transform.position).normalized;
            Quaternion bakisAcisi = Quaternion.LookRotation(new Vector3(yon.x, 0, yon.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, bakisAcisi, Time.deltaTime * 5f);

            animator.SetBool("isAttacking", true);
        }
        // BEKLEME (IDLE) DURUMU
        else
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isAttacking", false);
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

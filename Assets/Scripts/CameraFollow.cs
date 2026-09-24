using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private static CameraFollow Instance;

    [Header("Takip Edilecek Hedef")]
    [SerializeField] private Transform target;

    [Header("Kamera Açýsý ve Mesafesi")]
    [SerializeField] private float distance = 11f;      // hedefe olan mesafe
    [SerializeField][Range(20f, 85f)] private float pitchAngle = 55f; // 0 = tam yatay, 90 = tam tepeden

    [Header("Shake Ayarlarý")]
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    [Header("Yumuþaklýk Ayarý")]
    [SerializeField][Range(0.01f, 0.5f)] private float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;


    public void Shake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RecalculateOffset();
    }

    private void RecalculateOffset()
    {
        // Açýya göre offset'i otomatik hesapla (yukarý ve geriye doðru)
        float rad = pitchAngle * Mathf.Deg2Rad;
        float height = Mathf.Sin(rad) * distance;
        float back = Mathf.Cos(rad) * distance;

        offset = new Vector3(0f, height, -back);

        // Kamerayý bu açýya göre döndür (aþaðý bakacak þekilde)
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        Vector3 targetPosition = target.position + offset;

        // YENÝ: Shake varsa rastgele ofset ekle
        if (shakeTimer > 0f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.y = 0f; // dikey sarsýntýyý sýnýrlý tut, tamamen kaotik olmasýn
            targetPosition += shakeOffset;
            shakeTimer -= Time.deltaTime;
        }
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Ýnceleme/test kolaylýðý: Inspector'da deðer deðiþtirdiðinde anýnda görmek için
    private void OnValidate()
    {
        RecalculateOffset();
    }
}
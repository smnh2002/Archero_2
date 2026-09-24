using UnityEngine;
using TMPro;

/// <summary>
/// Hasar sayısını ekranda gösterip yukarı doğru hareket ettiren ve yavaşça silinen obje scripti.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private float disappearTimerMax = 1f;
    private Color textColor;
    private Vector3 moveVector;

    private Camera mainCam;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        mainCam = Camera.main;
    }

    /// <summary>
    /// Hasar popup'ını kurar.
    /// </summary>
    public void Setup(float damageAmount)
    {
        // Sayıyı tam sayıya çevir ve başına - koy (örn: -15)
        textMesh.SetText("-" + Mathf.RoundToInt(damageAmount).ToString());
        
        // Kırmızı renk (Kullanıcının isteği)
        textColor = new Color(1f, 0.15f, 0.15f, 1f); 
        textMesh.color = textColor;
        textMesh.fontSize = 5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Siyah çerçeve (Outline) ile daha okunaklı yapalım (Material destekliyorsa)
        textMesh.fontMaterial.EnableKeyword("OUTLINE_ON");
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = Color.black;

        disappearTimer = disappearTimerMax;

        // Yukarı ve hafif rastgele sağa/sola fırlama hareketi
        moveVector = new Vector3(Random.Range(-1f, 1f), Random.Range(2f, 3f), Random.Range(-0.5f, 0.5f));
        
        transform.localScale = Vector3.one * 0.5f; // Başlangıçta biraz küçük
    }

    private void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        // Her zaman kameraya baksın (Billboard)
        if (mainCam != null)
        {
            transform.rotation = mainCam.transform.rotation;
        }

        // Pozisyonu hareket ettir
        transform.position += moveVector * Time.deltaTime;
        
        // Yavaşça durmasını sağla (Friction)
        moveVector -= moveVector * 1.5f * Time.deltaTime;

        // İlk yarıda büyü, ikinci yarıda küçül
        if (disappearTimer > disappearTimerMax * 0.5f)
        {
            transform.localScale += Vector3.one * 1.5f * Time.deltaTime;
        }
        else
        {
            transform.localScale -= Vector3.one * 1.0f * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        
        // Süre dolduğunda fade out (saydamlaşma)
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            
            // Tamamen görünmez olunca yok et
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

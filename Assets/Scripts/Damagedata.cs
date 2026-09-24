using UnityEngine;

/// <summary>
/// Bir hasar olayýnýn tüm bilgisini taþýr.
/// Struct kullanýyoruz çünkü sýk sýk oluþturulup atýlacak (GC baskýsýný azaltmak için).
/// </summary>
public struct DamageData
{
    public float amount;           // Ne kadar hasar
    public Vector3 sourcePosition; // Hasarýn geldiði nokta (knockback yönü için)
    public float knockbackForce;   // Ne kadar geri itilecek
    public bool isCritical;        // Ýleride kritik vuruþ / damage number rengi için

    public DamageData(float amount, Vector3 sourcePosition, float knockbackForce = 0f, bool isCritical = false)
    {
        this.amount = amount;
        this.sourcePosition = sourcePosition;
        this.knockbackForce = knockbackForce;
        this.isCritical = isCritical;
    }
}
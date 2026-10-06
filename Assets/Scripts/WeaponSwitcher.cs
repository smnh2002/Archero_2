using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Silah Referansları")]
    public GameObject sword;
    public GameObject shield;
    public GameObject axe;

    void Update()
    {
        // Klavye yoksa hata vermemesi için kontrol edelim
        if (Keyboard.current == null) return;

        // 1 tuşuna basıldığında (Baltaya geç)
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (sword != null) sword.SetActive(false);
            if (shield != null) shield.SetActive(false);
            if (axe != null) axe.SetActive(true);
        }

        // 2 tuşuna basıldığında (Kılıç ve Kalkana geri dön)
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (sword != null) sword.SetActive(true);
            if (shield != null) shield.SetActive(true);
            if (axe != null) axe.SetActive(false);
        }
    }
}

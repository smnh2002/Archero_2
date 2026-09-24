using UnityEngine;
using TMPro;

/// <summary>
/// Hasar yazılarını (Damage Popup) oyun dünyasında dinamik olarak oluşturur.
/// Statik metod içerdiği için herhangi bir scriptten DamagePopupManager.Create(...) olarak çağrılabilir.
/// </summary>
public static class DamagePopupManager
{
    public static void Create(Vector3 position, float damageAmount)
    {
        // Popup için yeni bir boş GameObject oluştur
        GameObject popupObj = new GameObject("DamagePopup");
        
        // Düşmanın hafif üzerinde çıkması için pozisyonu yukarı taşı
        popupObj.transform.position = position + new Vector3(0f, 2.5f, 0f);

        // TextMeshPro (3D) bileşenini ekle
        TextMeshPro textMesh = popupObj.AddComponent<TextMeshPro>();
        
        // Font asset ayarları (Eğer default font sorun çıkarırsa, Resources'dan font yüklenebilir)
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Ekstra netlik için ayarlar
        textMesh.isOrthographic = false;
        
        // DamagePopup scriptini ekle ve çalıştır
        DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(damageAmount);
        
        // Katman olarak ön planda gözükmesi için Sorting Order ayarla
        textMesh.sortingOrder = 50;
    }
}

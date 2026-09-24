using UnityEngine;

/// <summary>
/// Bu script, üzerine eklendiği objenin (Özellikle World Space Canvas'ların)
/// her zaman kameraya tam olarak bakmasını (Screen-Aligned) sağlar.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // Objeyi kameranın baktığı yöne doğru (aynı açıyla) hizala.
        // Bu sayede Canvas, ekrana tam paralel olur ve ters/aynalı gözükmez.
        transform.rotation = mainCam.transform.rotation;
    }
}

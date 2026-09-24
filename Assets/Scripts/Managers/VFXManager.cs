using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private GameObject bloodHitEffectPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayBloodEffect(Vector3 position)
    {
        if (bloodHitEffectPrefab == null) return;

        if (PoolManager.Instance != null)
            PoolManager.Instance.Get(bloodHitEffectPrefab, position, Quaternion.identity);
        else
            Instantiate(bloodHitEffectPrefab, position, Quaternion.identity);
    }
}
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BloodEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem ps;
    private float lifetime;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
    }

    public void OnSpawnFromPool()
    {
        ps.Clear();
        ps.Play();

        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnDelayed(gameObject, lifetime);
        else
            Destroy(gameObject, lifetime);
    }

    public void OnReturnToPool()
    {
        ps.Stop();
    }
}
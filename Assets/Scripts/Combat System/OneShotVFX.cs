using UnityEngine;

public class OneShotVFX : MonoBehaviour
{
    private ParticleSystem ps;
    private float lifetime;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
    }

    private void OnEnable()
    {
        ps.Clear();
        ps.Play();
        Destroy(gameObject, lifetime);
    }
}
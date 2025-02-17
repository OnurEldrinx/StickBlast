using UnityEngine;
using UnityEngine.Pool;

public class PoolObject : MonoBehaviour
{
    public IObjectPool<ParticleSystem> Pool;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleSystemStopped()
    {
        Pool.Release(_particleSystem);
    }
}

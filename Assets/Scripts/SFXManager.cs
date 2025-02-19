using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] private AudioClip pieceDrop;
    [SerializeField] private AudioClip cellFill;
    [SerializeField] private AudioClip blast;

    private AudioSource _audioSource;

    private readonly Dictionary<SfxType, SfxConfig> _sfxConfigs = new();

    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        _sfxConfigs[SfxType.Drop] = new SfxConfig(pieceDrop, randomPitch: true);
        _sfxConfigs[SfxType.Fill] = new SfxConfig(cellFill, stopBeforePlaying: true, delay: 0.2f);
        _sfxConfigs[SfxType.Blast] = new SfxConfig(blast, stopBeforePlaying: true, randomPitch: true);
    }
    
    public void PlaySfx(SfxType type)
    {
        if (!_sfxConfigs.TryGetValue(type, out var config))
        {
            return;
        }

        if (config.StopBeforePlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.pitch = config.RandomPitch ? Random.Range(0.9f, 1.1f) : 1f;

        if (config.Delay > 0f)
        {
            _audioSource.clip = config.Clip;
            _audioSource.PlayDelayed(config.Delay);
        }
        else
        {
            _audioSource.PlayOneShot(config.Clip);
        }
    }
    
}

public enum SfxType
{
    Drop,
    Fill,
    Blast
}

public class SfxConfig
{
    public AudioClip Clip { get; }
    public bool StopBeforePlaying { get; }
    public bool RandomPitch { get; }
    public float Delay { get; }

    public SfxConfig(AudioClip clip, bool stopBeforePlaying = false, bool randomPitch = false, float delay = 0f)
    {
        Clip = clip;
        StopBeforePlaying = stopBeforePlaying;
        RandomPitch = randomPitch;
        Delay = delay;
    }
}

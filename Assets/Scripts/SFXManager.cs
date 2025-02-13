using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] private AudioClip pieceDrop;
    [SerializeField] private AudioClip cellFill;
    [SerializeField] private AudioClip blast;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySfx(SfxType type)
    {
        
        switch (type)
        {
            case SfxType.Drop:
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(pieceDrop);
                break;
            case SfxType.Fill:
                _audioSource.Stop();
                _audioSource.clip = cellFill;
                _audioSource.pitch = 1f;
                _audioSource.PlayDelayed(0.2f);
                break;
            case SfxType.Blast:
                _audioSource.Stop();
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(blast);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
}

public enum SfxType
{
    Drop,
    Fill,
    Blast
}

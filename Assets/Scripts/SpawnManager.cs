using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private SpriteRenderer fillSprite;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Draggable> draggablePrefabs;
    [SerializeField] private ParticleSystem blastEffect;
    [SerializeField] public List<Draggable> piecesInTray;
        
    public int draggableCount;

    private IObjectPool<ParticleSystem> _blastEffectPool;
    private bool _collectionCheck = true;
    
    private void Awake()
    {
        _blastEffectPool = new ObjectPool<ParticleSystem>(OnCreateBlastEffect,OnGetBlastEffect,OnReleaseBlastEffect,OnDestroyBlastEffect,_collectionCheck,6,12);
    }

    private void OnDestroyBlastEffect(ParticleSystem obj)
    {
        Destroy(obj);
    }

    private void OnReleaseBlastEffect(ParticleSystem obj)
    {
        obj.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        obj.gameObject.SetActive(false);
    }

    private void OnGetBlastEffect(ParticleSystem obj)
    {
        obj.gameObject.SetActive(true);
    }

    private ParticleSystem OnCreateBlastEffect()
    {
        var b = Instantiate(blastEffect);
        b.GetComponent<PoolObject>().Pool = _blastEffectPool;
        return b;
    }

    private void Start()
    { 
        SpawnDraggables();
    }
    
    public void FillTheCell(Transform cell, Color c)
    {
        if (fillSprite == null)
        {
            return;
        }
        
        SpriteRenderer f = Instantiate(fillSprite, cell, true);
        f.color = c;
        f.transform.localPosition = Vector3.zero;
        f.transform.DOScale(Vector3.one * 1.175f, 0.5f);


        cell.GetComponent<Cell>().fill = f.gameObject;

        SFXManager.Instance.PlaySfx(SfxType.Fill);
        
    }
    
    private void SpawnDraggables()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return;
        }
        if (draggablePrefabs == null || draggablePrefabs.Count == 0)
        {
            return;
        }

        draggableCount = 0;
        foreach (var spawnPoint in spawnPoints)
        {
            Draggable prefab = draggablePrefabs[Random.Range(0, draggablePrefabs.Count)];
            if (prefab == null)
            {
                continue;
            }
            
            Draggable d = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            d.transform.localScale = Vector3.zero;
            
            d.transform.DOScale(d.data.spawnScale * Vector3.one, 0.5f).SetEase(Ease.OutBack);
            draggableCount++;
            
            piecesInTray.Add(d);
            
        }
    }
    
    public void UpdateTray(Draggable d)
    {
        if (piecesInTray.Contains(d))
        {
            piecesInTray.Remove(d);
        }
            
        draggableCount = Mathf.Max(0, draggableCount - 1);
        if (draggableCount <= 0)
        {
            draggableCount = 0;
            SpawnDraggables();
        }
            
        FailCheckManager.Instance.CheckFailCondition(piecesInTray);
    }

    public void SpawnBlastEffect(Vector3 position,bool isColumnBlast)
    {
        //var b = Instantiate(blastEffect, position, Quaternion.identity);
        var b = _blastEffectPool.Get();
        b.transform.position = position;
        b.transform.rotation = Quaternion.Euler(0, 180, isColumnBlast ? 90 : 0);
        b.Play();
        SFXManager.Instance.PlaySfx(SfxType.Blast);
        
    }
    
}

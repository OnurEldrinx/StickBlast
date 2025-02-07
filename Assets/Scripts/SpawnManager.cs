using DG.Tweening;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private SpriteRenderer fillSprite;

    public void FillTheCell(Transform cell,Color c)
    {
        var f = Instantiate(fillSprite,cell,true);
        f.color = c;
        f.transform.localPosition = Vector3.zero;
        f.transform.DOScale(Vector3.one*1.175f, 0.5f);
        
    }
    
}

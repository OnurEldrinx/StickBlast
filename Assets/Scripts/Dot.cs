using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [SerializeField] public int id;
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] public List<Dot> neighbors;
    
    public List<Cell> Cells { get; private set; }
    public List<Edge> Edges { get; private set; }

    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;
    private Color _initialColor;

    
    private Tweener _highlightFadeTween;
    
    private Tweener _fillColorTween;
    
    public bool highlighted;
    

    private void Awake()
    {
        Cells = new List<Cell>();
        Edges = new List<Edge>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
        _initialColor = _spriteRenderer.color;
    }
    
    public void SetCoordinates(Vector2Int c)
    {
        coordinates = c;
    }

    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }

    public void Highlight(Color c)
    {
        if (highlighted) { return; }

        _highlightFadeTween.Kill();
        _spriteRenderer.color = c;
        highlighted = true;
    }

    public void Unhighlight()
    {
        if(_spriteRenderer is null || !highlighted){return;}
        _highlightFadeTween = _spriteRenderer?.DOColor(_defaultColor, 0.25f);
        highlighted = false;

    }

    public void FillAnimation(Color c)
    {
        if(_spriteRenderer is null){return;}
        transform.DOShakeScale(0.5f, 0.5f, 10, 0, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutBounce);
        _fillColorTween = _spriteRenderer?.DOColor(c, 0.5f).SetDelay(0.1f);
        _defaultColor = c;
    }

    public void ResetState()
    {
        //DOTween.Kill(_fillColorTween);
        _fillColorTween.Kill();
        if(_spriteRenderer is null){return;}
        _spriteRenderer?.DOColor(_initialColor, 0.25f);
        _defaultColor = _initialColor;
        highlighted = false;
    }
    

    
    
}

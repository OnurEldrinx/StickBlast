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
    
    

    private void Awake()
    {
        Cells = new List<Cell>();
        Edges = new List<Edge>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
        _initialColor = _spriteRenderer.color;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            dotSensor.SetSnapPosition(transform.position);
            dotSensor.SpecifySnapTarget(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            Unhighlight();
            dotSensor.SpecifySnapTarget(null);
        }
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
        _highlightFadeTween.Kill();
        _spriteRenderer.color = c;
    }

    private void Unhighlight()
    {
        if(_spriteRenderer is null){return;}
        _highlightFadeTween = _spriteRenderer?.DOColor(_defaultColor, 0.25f);
    }

    public void FillAnimation(Color c)
    {
        if(_spriteRenderer is null){return;}
        transform.DOShakeScale(0.5f, 0.5f, 10, 0, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutBounce);
        _fillColorTween = _spriteRenderer?.DOColor(c, 0.5f);
        _defaultColor = c;
    }

    public void ResetState()
    {
        DOTween.Kill(_fillColorTween);
        if(_spriteRenderer is null){return;}
        _spriteRenderer?.DOColor(_initialColor, 0.25f);
        _defaultColor = _initialColor;
    }

    public void InstantReset()
    {
        if(_spriteRenderer is null){return;}
        _spriteRenderer.color = _initialColor;
        _defaultColor = _initialColor;
    }

    
    
}

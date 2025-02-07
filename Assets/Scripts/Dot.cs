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

    private int _defaultSortingOrder;
    private int _currentSortingOrder;
    
    private Tweener _highlightFadeTween;

    private void Awake()
    {
        Cells = new List<Cell>();
        Edges = new List<Edge>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
        _initialColor = _spriteRenderer.color;
        _defaultSortingOrder = _spriteRenderer.sortingOrder;
        _currentSortingOrder = _defaultSortingOrder;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            //_spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            dotSensor.SetSnapPosition(transform.position);
            //dotSensor.SnapTargetAvailable = true;
            dotSensor.SpecifySnapTarget(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            Unhighlight();
            //dotSensor.SnapTargetAvailable = false;
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

    public void Unhighlight()
    {
        //_spriteRenderer.color = _defaultColor;
        _highlightFadeTween = _spriteRenderer.DOColor(_defaultColor,0.25f);

    }

    public void FillAnimation(Color c)
    {
        transform.DOShakeScale( 0.5f,0.5f,10,0,true,ShakeRandomnessMode.Harmonic).SetEase(Ease.OutBounce);
        _spriteRenderer.DOColor(c,0.5f);
        _defaultColor = c;

    }

    public void ResetState()
    {
        _spriteRenderer.color = _initialColor;
        _defaultColor = _initialColor;
    }
    
}

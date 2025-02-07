using System.Collections.Generic;
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

    private int _defaultSortingOrder;
    private int _currentSortingOrder;

    private void Awake()
    {
        Cells = new List<Cell>();
        Edges = new List<Edge>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
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

    public void Highlight()
    {
        _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    public void Unhighlight()
    {
        _spriteRenderer.color = _defaultColor;
    }

    public void BringToFront()
    {
        _spriteRenderer.sortingOrder = 5;
    }

    public void BringToBack()
    {
        _spriteRenderer.sortingOrder = 2;
    } 
    
}

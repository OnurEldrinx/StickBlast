using UnityEngine;

public class DotSensor : MonoBehaviour
{
    private Vector3 _snapPosition;
    public bool SnapTargetAvailable { get; private set; }
    public Dot snapTarget;
    
    private BoxCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    
    public Vector2Int localCoordinate;
    public bool isAnchor;

    public float snapThreshold = 0.55f;
    
    private Dot _lastHighlightedDot;
    private Dot _nearestDot;
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _spriteRenderer.sortingOrder = 15;

        isAnchor = (localCoordinate == Vector2.zero);
        
    }
    
    public void SearchForSnapTarget()
    {
        Vector2 sensorPos = transform.position;
        _nearestDot = null;
        var minDistance = snapThreshold;
        
        foreach (Dot dot in GridManager.Instance.dots)
        {
            float distance = Vector2.Distance(sensorPos, dot.transform.position);
            if (distance <= minDistance)
            {
                minDistance = distance;
                _nearestDot = dot;
            }
            else
            {
                dot.Unhighlight();
            }
        }
        
        if (_nearestDot)
        {
            SetSnapPosition(_nearestDot.transform.position);
            SpecifySnapTarget(_nearestDot);
        }
        else
        {
            SpecifySnapTarget(null);
        }
    }
    

    public void Initialize(Color c)
    {
        _spriteRenderer.color = c;
    }
    
    private void SetSnapPosition(Vector3 p)
    {
        _snapPosition = p;
    }
    
    public Vector3 GetSnapPosition()
    {
        return _snapPosition;
    }
    
    private void SpecifySnapTarget(Dot target)
    {
        snapTarget = target;
        SnapTargetAvailable = target;
        _snapPosition = target ? target.transform.position : Vector3.zero;
    }
    
    public void SetColliderState(bool state)
    {
        _collider.enabled = state;
    }
    
    public void EnableSpriteRenderer(bool state)
    {
        _spriteRenderer.enabled = state;
    }
    
    public void ResetSensor()
    {
        snapTarget = null;
        _snapPosition = Vector3.zero;
        SnapTargetAvailable = false;
    }
}

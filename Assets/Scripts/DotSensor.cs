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
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _spriteRenderer.sortingOrder = 15;

        isAnchor = (localCoordinate == Vector2.zero);
        
    }
    
    public void Initialize(Color c)
    {
        _spriteRenderer.color = c;
    }
    
    public void SetSnapPosition(Vector3 p)
    {
        _snapPosition = p;
    }
    
    public Vector3 GetSnapPosition()
    {
        return _snapPosition;
    }
    
    public void SpecifySnapTarget(Dot target)
    {
        snapTarget = target;
        SnapTargetAvailable = target != null;
        _snapPosition = target != null ? target.transform.position : Vector3.zero;
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

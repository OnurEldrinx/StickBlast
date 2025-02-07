using System;
using UnityEngine;

public class DotSensor : MonoBehaviour
{
    private Vector3 _snapPosition;
    public bool SnapTargetAvailable { get; set; }
    public Dot snapTarget;
    
    private BoxCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
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
        SnapTargetAvailable = target is not null;
        
    }

    public void SetColliderState(bool state)
    {
        _collider.enabled = state;
    }
    
    
    
}

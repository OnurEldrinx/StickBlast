using UnityEngine;
using DG.Tweening;

public class DotSensor : MonoBehaviour
{
    private Vector3 _snapPosition;
    // Expose SnapTargetAvailable as read-only to external scripts.
    public bool SnapTargetAvailable { get; private set; }
    public Dot snapTarget;
    
    private BoxCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _spriteRenderer.sortingOrder = 15;
    }

    /// <summary>
    /// Initializes the sensor's sprite with the given color.
    /// </summary>
    public void Initialize(Color c)
    {
        _spriteRenderer.color = c;
    }
    
    /// <summary>
    /// Sets the position where this sensor will snap.
    /// </summary>
    public void SetSnapPosition(Vector3 p)
    {
        _snapPosition = p;
    }

    /// <summary>
    /// Returns the designated snap position.
    /// </summary>
    public Vector3 GetSnapPosition()
    {
        return _snapPosition;
    }

    /// <summary>
    /// Specifies the snap target for this sensor and updates availability.
    /// </summary>
    public void SpecifySnapTarget(Dot target)
    {
        snapTarget = target;
        SnapTargetAvailable = target != null;
        _snapPosition = target != null ? target.transform.position : Vector3.zero;
    }

    /// <summary>
    /// Enables or disables the sensor's collider.
    /// </summary>
    public void SetColliderState(bool state)
    {
        _collider.enabled = state;
    }

    /// <summary>
    /// Enables or disables the sensor's sprite renderer.
    /// </summary>
    public void EnableSpriteRenderer(bool state)
    {
        _spriteRenderer.enabled = state;
    }
    
    /// <summary>
    /// Plays an animation on the sensor to indicate that it has been filled.
    /// </summary>
    public void FillAnimation(Color c)
    {
        // Shake the scale of the sensor's transform.
        transform.DOShakeScale(0.5f, 0.5f, 10, 0, true, ShakeRandomnessMode.Harmonic)
                 .SetEase(Ease.OutBounce);
        // Tween the sprite's color to the provided color.
        _spriteRenderer.DOColor(c, 0.5f);
    }

    /// <summary>
    /// Resets the sensor's state.
    /// Renamed from Reset() to ResetSensor() to avoid conflict with Unity's built-in Reset method.
    /// </summary>
    public void ResetSensor()
    {
        snapTarget = null;
        _snapPosition = Vector3.zero;
        SnapTargetAvailable = false;
    }
}

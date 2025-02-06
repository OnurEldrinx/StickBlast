using UnityEngine;

public class Dot : MonoBehaviour
{
    [SerializeField] private int emptyEdgeCount = 4;
    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            _spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            dotSensor.SetSnapPosition(transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<DotSensor>(out var dotSensor))
        {
            _spriteRenderer.color = _defaultColor;
        }
    }
}

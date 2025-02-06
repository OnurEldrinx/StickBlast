using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    
    
    private Camera _mainCamera;
    private Vector3 _startPosition;
    [SerializeField] private float dragOffset;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private DotSensor[] dotSensors;
    
    private Dot _nearestDot;
    
    private BoxCollider2D _collider;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        _collider = GetComponent<BoxCollider2D>();
        dotSensors = GetComponentsInChildren<DotSensor>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPosition = transform.position;
        
        // Give offset
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);
        
        
        /*var hit = Physics2D.Raycast(transform.position, transform.forward, 10,gridLayer);
        if (hit.collider != null)
        {
            _nearestDot = GridManager.Instance.NearestDot(hit.point);
            _nearestDot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }*/

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //print($"OnPointerUp {gameObject.name}");
        
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();
        foreach (var sensor in dotSensors)
        {
            var p = sensor.GetSnapPosition();
            xPositions.Add(p.x);
            yPositions.Add(p.y);
        }
        
        var targetX = (xPositions.Min() + xPositions.Max())/2;
        var targetY = (yPositions.Min() + yPositions.Max())/2;
        
        transform.position = new Vector3(targetX, targetY, transform.position.z);
        
    }
}

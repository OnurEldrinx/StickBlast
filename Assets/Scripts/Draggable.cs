using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Camera _mainCamera;
    private Vector3 _startPosition;
    [SerializeField] private float dragOffset;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private DotSensor[] dotSensors;
    [SerializeField] private List<ShapeEdge> edges;
    [SerializeField] private Transform ghostTransform;
    [SerializeField] public ShapeData data;
    [SerializeField] private List<ShapeEdge> shapeEdges = new();

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private ThemeData _theme;
    private Tweener _offsetTween;
    private Tweener _scaleTween;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        dotSensors = GetComponentsInChildren<DotSensor>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _theme = data.theme;
        dragOffset = data.dragOffset;
        gridLayer = data.gridLayer;
        _spriteRenderer.sprite = data.sprite;
        _spriteRenderer.enabled = false;

        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.defaultSortingOrder;
            shapeEdge.sprite.color = _theme.defaultColor;
        }

        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.edgeRadius = 0.25f;
    }

    private void Start()
    {
        for (int i = 0; i < dotSensors.Length - 1; i++)
        {
            var edge = new ShapeEdge(dotSensors[i], dotSensors[i + 1]);
            edges.Add(edge);
        }

        CreateGhost();

        foreach (var sensor in dotSensors)
        {
            sensor.Initialize(_theme.dotColor);
        }
        
        _startPosition = transform.position;
    }

    private void CreateGhost()
    {
        GameObject ghost = new GameObject
        {
            name = "Ghost",
            transform =
            {
                parent = transform,
                localPosition = Vector3.zero
            }
        };
        var ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = data.sprite;
        ghostRenderer.color = new Color(1f, 1f, 1f, 0.3f);
        ghostRenderer.sortingOrder = 3;

        ghostTransform = ghost.transform;
        ghostTransform.localScale = Vector3.one;
        ghost.SetActive(false);
    }

    
    
    public void OnPointerDown(PointerEventData eventData)
    {
        
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        _offsetTween = transform.DOMove(pointerPosition + (Vector3.up * dragOffset),0.1f);
        

        _scaleTween = transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.InOutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);
        
        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        if (!canDrop)
        {
            UnhighlightSensors();
            return;
        }

        foreach (var sensor in dotSensors)
        {
            sensor.SearchForSnapTarget();
        }

        bool candidatePlaceAvailable = IsCandidatePlaceAvailable();

        if (candidatePlaceAvailable)
            HighlightSensors();
        else
            UnhighlightSensors();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _offsetTween.Kill();

        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        if (canDrop)
            Drop();
        else
            BackToStart();
    }

    
    private bool IsCandidatePlaceAvailable()
    {
        var snapTargets = new List<Dot>();

        foreach (var sensor in dotSensors)
        {
            if (sensor.snapTarget == null)
                return false; 
            snapTargets.Add(sensor.snapTarget);
        }

        if (HasDuplicates(snapTargets))
        {
            return false;
        }
        

        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            Dot current = snapTargets[i];
            Dot next = snapTargets[i + 1];

            var edge = GridManager.Instance.GetEdge(current, new EdgeKey(current.id, next.id));

            if ((edge != null && edge.filled))
                return false;
        }
        return true;

        bool HasDuplicates<T>(IEnumerable<T> list)
        {
            HashSet<T> seen = new HashSet<T>();
            return list.Any(item => !seen.Add(item));
        }
    }
    
    private void HighlightSensors()
    {
        foreach (var sensor in dotSensors)
        {
            sensor.snapTarget?.Highlight(_theme.highlightColor);
        }

        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();

        foreach (var sensor in dotSensors)
        {
            var p = sensor.GetSnapPosition();
            xPositions.Add(p.x);
            yPositions.Add(p.y);
        }

        float targetX = (xPositions.Min() + xPositions.Max()) / 2;
        float targetY = (yPositions.Min() + yPositions.Max()) / 2;

        ghostTransform.position = new Vector3(targetX, targetY, ghostTransform.position.z);
        ghostTransform.gameObject.SetActive(true);
    }

    
    private void UnhighlightSensors()
    {
        ghostTransform.gameObject.SetActive(false);
        ghostTransform.localPosition = Vector3.zero;

        foreach (var sensor in dotSensors)
        {
            sensor.snapTarget?.Unhighlight();
        }
        
    }
    
    private void Drop()
    {
        if (!ghostTransform.gameObject.activeInHierarchy)
        {
            BackToStart();
            return;
        }
        
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();
        var snapTargets = new List<Dot>();
        var cellTargets = new HashSet<Cell>();
        var edgeTargets = new HashSet<EdgeKey>();

        foreach (var sensor in dotSensors)
        {
            if (!sensor.SnapTargetAvailable)
            {
                BackToStart();
                return;
            }

            var p = sensor.GetSnapPosition();
            xPositions.Add(p.x);
            yPositions.Add(p.y);

            Dot currentDot = sensor.snapTarget;
            snapTargets.Add(currentDot);
            cellTargets.UnionWith(currentDot.Cells);
        }

        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            //edgeTargets.Add($"{snapTargets[i].id},{snapTargets[i + 1].id}");
            edgeTargets.Add(new EdgeKey(snapTargets[i].id, snapTargets[i+1].id));
        }

        foreach (var cell in cellTargets)
        {
            if (cell.completed)
                continue;

            if (!cell.FillEdges(snapTargets, edgeTargets, _theme.defaultColor, shapeEdges))
            {
                BackToStart();
                return;
            }
        }

        foreach (var dot in snapTargets)
        {
            dot.FillAnimation(_theme.defaultColor);
        }

        float targetX = (xPositions.Min() + xPositions.Max()) / 2;
        float targetY = (yPositions.Min() + yPositions.Max()) / 2;
        transform.position = new Vector3(targetX, targetY, transform.position.z);

        foreach (var shapeEdge in shapeEdges)
        {
            var d1 = shapeEdge.d1.GetSnapPosition();
            var d2 = shapeEdge.d2.GetSnapPosition();
            float x = (d1.x + d2.x) / 2;
            float y = (d1.y + d2.y) / 2;
            shapeEdge.sprite.transform.position = new Vector3(x, y, transform.position.z);
        }

        _collider.enabled = false;
        foreach (var sensor in dotSensors)
        {
            sensor.SetColliderState(false);
        }
        BringToBack();
        ghostTransform.gameObject.SetActive(false);

        SpawnManager.Instance.UpdateTray(this);
        
        SFXManager.Instance.PlaySfx(SfxType.Drop);
    }

    private void BackToStart()
    {
        _offsetTween?.Kill();
        _scaleTween?.Kill();
        transform.position = _startPosition;
        transform.localScale = data.spawnScale * Vector3.one;
        UnhighlightSensors();
        foreach (var sensor in dotSensors)
        {
            sensor.ResetSensor();
        }
    }

    private void BringToBack()
    {
        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.ghostSortingOrder;
        }
    }

    private void OnDisable()
    {
        Invoke(nameof(ResetState), 1);
    }

    private void ResetState()
    {
        transform.parent = null;
        _collider.enabled = true;
        foreach (var sensor in dotSensors)
        {
            sensor.SetColliderState(true);
            sensor.EnableSpriteRenderer(false);
        }
        ghostTransform.gameObject.SetActive(false);
        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.defaultSortingOrder;
        }
    }

    public DotSensor[] GetDotSensors()
    {
        return dotSensors;
    }


    public bool CanPlaceOnGrid(Dot dot)
    {
        foreach (var sensor in dotSensors)
        {
            if (sensor.isAnchor)
            {
                sensor.snapTarget = dot;
                continue;
            }
            
            sensor.snapTarget = GridManager.Instance.GetDotWithOffset(dot, sensor.localCoordinate);
        }

        var result = IsCandidatePlaceAvailable();

        foreach (var sensor in dotSensors)
        {
            sensor.snapTarget = null;
        }
        
        return result;
    }
    
    private bool CanPlaceOnGridOnSpawn(Dot dot)
    {
        try
        {

            var snapTargets = new Dot[data.offsets.Length];
            int index = 0;
            foreach (var offset in data.offsets)
            {
                var currentDot = GridManager.Instance.GetDotWithOffset(dot, offset);
                snapTargets[index++] = currentDot;
                if (currentDot is null)
                {
                    return false;
                }
            }
            
            for (int i = 0; i < snapTargets.Length - 1; i++)
            {
                Dot current = snapTargets[i];
                Dot next = snapTargets[i + 1];

                var edge = GridManager.Instance.GetEdge(current, new EdgeKey(current.id, next.id));
                
                if ((edge != null && edge.filled))
                    return false;
            }
        
            return true;
        }
        catch (Exception e)
        {
            print(e.StackTrace);
            throw;
        }
        
    }

    public bool IsCandidatePlaceAvailableOnSpawn()
    { 
        var result = GridManager.Instance.CandidateDots().Any(CanPlaceOnGridOnSpawn);
        return result;
    }
    
    
}

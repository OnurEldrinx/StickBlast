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

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private ThemeData _theme;

    [SerializeField] private List<ShapeEdge> shapeEdges = new List<ShapeEdge>();

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
        // _spriteRenderer.color = _theme.defaultColor;
        // _spriteRenderer.sortingOrder = data.defaultSortingOrder;

        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.defaultSortingOrder;
            shapeEdge.sprite.color = _theme.defaultColor;
        }

        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.edgeRadius = 0.1f;
    }

    private void Start()
    {
        // Assuming each edge is defined between consecutive DotSensors.
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

    private Tweener _offsetTween;
    private Tweener _scaleTween;
    
    public void OnPointerDown(PointerEventData eventData)
    {

        if (eventData.clickCount == 1)
        {
            print("Clicked");
        }
        
        // Calculate the initial offset so the object does not “jump.”
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        //transform.position = pointerPosition + (Vector3.up * dragOffset);
        _offsetTween = transform.DOMove(pointerPosition + (Vector3.up * dragOffset),0.1f);
        

        //transform.localScale = Vector3.one;
        _scaleTween = transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.InOutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update object position to follow the pointer with the drag offset.
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);

        // Check if the object is over a valid grid cell.
        // (Using transform.forward is acceptable if your grid is on the appropriate plane.)
        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        // If not over a grid cell, remove any candidate highlighting.
        if (!canDrop)
        {
            UnhighlightSensors();
            return;
        }

        // Now check if the candidate placement is valid.
        bool candidatePlaceAvailable = IsCandidatePlaceAvailable();

        if (candidatePlaceAvailable)
            HighlightSensors();
        else
            UnhighlightSensors();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _offsetTween.Kill();
        // Check again for a valid grid cell under the piece.
        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        if (canDrop)
            Drop();
        else
            BackToStart();
    }

    /// <summary>
    /// Verifies that every DotSensor has a valid snap target and that none of the consecutive edges are already filled.
    /// </summary>
    /// <returns>true if placement is valid; otherwise, false.</returns>
    private bool IsCandidatePlaceAvailable()
    {
        var snapTargets = new List<Dot>();

        // Gather snap targets from all dot sensors.
        foreach (var sensor in dotSensors)
        {
            if (sensor.snapTarget == null)
                return false; // Placement is invalid if any sensor lacks a target.
            snapTargets.Add(sensor.snapTarget);
        }

        // Check each consecutive pair to ensure the corresponding grid edge is available.
        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            Dot current = snapTargets[i];
            Dot next = snapTargets[i + 1];

            // Assume GridManager.GetEdge returns an edge by a key made from dot IDs.
            var edge = GridManager.Instance.GetEdge(current, current.id + "," + next.id);
            var edgeAlternative = GridManager.Instance.GetEdge(current, next.id + "," + current.id);

            // If either edge is already filled, the candidate placement fails.
            if ((edge != null && edge.filled) || (edgeAlternative != null && edgeAlternative.filled))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Highlights all snap targets and moves the ghost transform to the center of the candidate placement.
    /// </summary>
    private void HighlightSensors()
    {
        foreach (var sensor in dotSensors)
        {
            sensor.snapTarget?.Highlight(_theme.highlightColor);
        }

        // Calculate the new center position based on snap target positions.
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

    /// <summary>
    /// Disables highlighting.
    /// </summary>
    private void UnhighlightSensors()
    {
        // (If you have an Unhighlight() method on your sensors, call it here.)
        ghostTransform.gameObject.SetActive(false);
        ghostTransform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Drops the shape if all sensors are properly snapped. After a successful drop,
    /// it calls the grid blast method to clear any full row/column.
    /// </summary>
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
        var edgeTargets = new HashSet<string>();

        // Gather snap positions, dots, and associated grid cells.
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

        // Create keys for edges between consecutive dots.
        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            edgeTargets.Add($"{snapTargets[i].id},{snapTargets[i + 1].id}");
        }

        // For every cell associated with this shape, attempt to fill the required edges.
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

        // Animate filling for each dot.
        foreach (var dot in snapTargets)
        {
            dot.FillAnimation(_theme.defaultColor);
        }

        // Reposition the shape to the center of its snap targets.
        float targetX = (xPositions.Min() + xPositions.Max()) / 2;
        float targetY = (yPositions.Min() + yPositions.Max()) / 2;
        transform.position = new Vector3(targetX, targetY, transform.position.z);

        // Update positions of each shape edge sprite.
        foreach (var shapeEdge in shapeEdges)
        {
            var d1 = shapeEdge.d1.GetSnapPosition();
            var d2 = shapeEdge.d2.GetSnapPosition();
            float x = (d1.x + d2.x) / 2;
            float y = (d1.y + d2.y) / 2;
            shapeEdge.sprite.transform.position = new Vector3(x, y, transform.position.z);
        }

        // Disable interaction and update visuals.
        _collider.enabled = false;
        foreach (var sensor in dotSensors)
        {
            sensor.SetColliderState(false);
        }
        BringToBack();
        ghostTransform.gameObject.SetActive(false);

        // ***** NEW CODE: Check for and blast any fully filled row/column *****
        

        // Update the tray (or spawn new pieces).
        SpawnManager.Instance.UpdateTray();
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
        // _spriteRenderer.sortingOrder = data.ghostSortingOrder;
        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.ghostSortingOrder;
        }
    }

    private void OnDisable()
    {
        // Optionally, delay reset to allow for any animations to finish.
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
        // _spriteRenderer.sortingOrder = data.defaultSortingOrder;
        ghostTransform.gameObject.SetActive(false);
        foreach (var shapeEdge in shapeEdges)
        {
            shapeEdge.sprite.sortingOrder = data.defaultSortingOrder;
        }
    }
}

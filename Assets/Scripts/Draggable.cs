using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private ShapeData data;
    
    // This field is no longer needed globally; see the refactored check below.
    // private bool _candidatePlaceExist;

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private ThemeData _theme;

    
    private void Awake()
    {
        _mainCamera = Camera.main;
        dotSensors = GetComponentsInChildren<DotSensor>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _theme = data.theme;
        dragOffset = data.dragOffset;
        gridLayer = data.gridLayer;
        _spriteRenderer.sprite = data.sprite;
        _spriteRenderer.color = _theme.defaultColor;
        
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
        ghost.SetActive(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _startPosition = transform.position;

        // Calculate the initial offset so the object does not “jump.”
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update object position to follow the pointer with the drag offset.
        var pointerPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
        pointerPosition.z = transform.position.z;
        transform.position = pointerPosition + (Vector3.up * dragOffset);

        // Check if the object is over a valid grid cell.
        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        // If the object isn’t over a grid cell, you may choose to disable candidate highlighting.
        if (!canDrop)
        {
            UnhighlightSensors();
            return;
        }

        // Now check if the candidate placement is valid.
        bool candidatePlaceAvailable = IsCandidatePlaceAvailable();

        // If so, highlight the snap targets.
        if (candidatePlaceAvailable)
        {
            HighlightSensors();
        }
        else
        {
            UnhighlightSensors();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If over a valid grid cell and all sensors are properly snapped, drop the shape.
        // Otherwise, return to the original position.
        var hit = Physics2D.Raycast(transform.position, transform.forward, 10, gridLayer);
        bool canDrop = hit.collider != null;

        if (canDrop)
        {
            Drop();
        }
        else
        {
            BackToStart();
        }
    }

    /// <summary>
    /// Checks whether all dot sensors have a valid snap target and whether all consecutive edges are available.
    /// </summary>
    /// <returns>true if placement is valid; otherwise, false.</returns>
    private bool IsCandidatePlaceAvailable()
    {
        var snapTargets = new List<Dot>();

        // Gather snap targets from all dot sensors.
        foreach (var sensor in dotSensors)
        {
            if (sensor.snapTarget == null)
                return false; // If any sensor does not have a target, placement is invalid.

            snapTargets.Add(sensor.snapTarget);
        }

        // Check each consecutive pair to see if the corresponding grid edge is already filled.
        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            Dot current = snapTargets[i];
            Dot next = snapTargets[i + 1];

            // Assuming GridManager.GetEdge takes two dots (or one dot and the other’s id)
            // Adjust the call as needed. Here we assume it takes two Dot objects.
            var edge = GridManager.Instance.GetEdge(current, current.id+","+next.id);
            var edgeAlternative = GridManager.Instance.GetEdge(current, next.id+","+current.id);

            // If the edge exists and is filled, placement is not allowed.
            if (edge is { filled: true } || edgeAlternative is {filled:true}) return false;
        }

        return true;
    }

    /// <summary>
    /// Highlights each snap target by calling its Highlight method.
    /// </summary>
    private void HighlightSensors()
    {
        foreach (var sensor in dotSensors)
        {
            sensor.snapTarget?.Highlight(_theme.highlightColor);
        }
        
        // Calculate the new center position based on the min and max snap positions.
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();
        
        foreach (var sensor in dotSensors)
        {
            // Get the position where the sensor should snap.
            var p = sensor.GetSnapPosition();
            xPositions.Add(p.x);
            yPositions.Add(p.y);
        }
        
        
        var targetX = (xPositions.Min() + xPositions.Max()) / 2;
        var targetY = (yPositions.Min() + yPositions.Max()) / 2;
    
        ghostTransform.position = new Vector3(targetX, targetY, ghostTransform.position.z);
        ghostTransform.gameObject.SetActive(true);
    }

    /// <summary>
    /// Optionally, you can add logic here to remove highlighting.
    /// </summary>
    private void UnhighlightSensors()
    {
        /*foreach (var sensor in dotSensors)
        {
            sensor.snapTarget?.Unhighlight(); // Assumes an Unhighlight method exists.
        }*/
        ghostTransform.gameObject.SetActive(false);
        ghostTransform.localPosition = Vector3.zero;
    }

    private void Drop()
    {
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();
        var snapTargets = new List<Dot>();
        var cellTargets = new HashSet<Cell>();
        var edgeTargets = new HashSet<string>();

        foreach (var sensor in dotSensors)
        {
            // If any sensor can’t snap, cancel the drop.
            if (!sensor.SnapTargetAvailable)
            {
                BackToStart();
                return;
            }

            // Get the position where the sensor should snap.
            var p = sensor.GetSnapPosition();
            xPositions.Add(p.x);
            yPositions.Add(p.y);

            Dot currentDot = sensor.snapTarget;
            snapTargets.Add(currentDot);

            // Aggregate cells that are associated with this dot.
            cellTargets.UnionWith(currentDot.Cells);
        }

        // Create a set of edge keys based on consecutive dots.
        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            edgeTargets.Add($"{snapTargets[i].id},{snapTargets[i + 1].id}");
        }

        // Fill Logic: For every cell associated with the shape, ensure that all
        // the required edges can be filled. If any cell fails, cancel the drop.
        foreach (var cell in cellTargets)
        {
            if (!cell.FillEdges(snapTargets, edgeTargets,_theme.defaultColor))
            {
                BackToStart();
                return;
            }
        }

        foreach (var dot in snapTargets)
        {
            dot.FillAnimation(_theme.defaultColor);
        }
        

        // Calculate the new center position based on the min and max snap positions.
        var targetX = (xPositions.Min() + xPositions.Max()) / 2;
        var targetY = (yPositions.Min() + yPositions.Max()) / 2;

        transform.position = new Vector3(targetX, targetY, transform.position.z);

        // Disable interaction and update visuals.
        _collider.enabled = false;
        foreach (var sensor in dotSensors)
        {
            sensor.Disable();
        }
        BringToBack();
        ghostTransform.gameObject.SetActive(false);
    }

    private void BackToStart()
    {
        transform.position = _startPosition;
        UnhighlightSensors();
    }

    private void BringToBack()
    {
        _spriteRenderer.sortingOrder = 3;
    }
}

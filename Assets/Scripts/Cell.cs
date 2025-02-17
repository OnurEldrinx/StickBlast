using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] public List<Dot> dots; // Representing corners of this cell.
    [SerializeField] public List<Edge> edges;
    public bool completed;
    
    [SerializeField] private int edgesCount;

    public int id;
    [SerializeField] private Vector2Int coordinates;

    public GameObject fill;
    
    
    private void Start()
    {
        foreach (var dot in dots)
        {
            dot.Cells.Add(this);
        }
        
        DefineEdges();
    }
    
    private void DefineEdges()
    {
        edges = new List<Edge>();

        foreach (var d in dots)
        {
            Dot xMatch = dots.Find(n => n != d && n.GetCoordinates().x == d.GetCoordinates().x);
            if (xMatch != null)
            {
                if (!edges.Any(e => (e.d1 == d && e.d2 == xMatch) || (e.d1 == xMatch && e.d2 == d)))
                {
                    edges.Add(new Edge(d, xMatch,this));
                }
            }

            Dot yMatch = dots.Find(n => n != d && n.GetCoordinates().y == d.GetCoordinates().y);
            if (yMatch != null)
            {
                if (!edges.Any(e => (e.d1 == d && e.d2 == yMatch) || (e.d1 == yMatch && e.d2 == d)))
                {
                    edges.Add(new Edge(d, yMatch,this));
                }
            }
        }
        
        
        edgesCount = edges.Count;
    }
    
    public bool FillEdges(List<Dot> targetDots, HashSet<string> targetEdges, Color fillColor, List<ShapeEdge> sticks)
    {
        foreach (var shapeEdge in sticks)
        {
            if (shapeEdge.d1.snapTarget != null && shapeEdge.d2.snapTarget != null)
            {
                shapeEdge.currentEdgeTag = shapeEdge.d1.snapTarget.id + "," + shapeEdge.d2.snapTarget.id;
            }
        }
        
        HashSet<Dot> dotSet = new HashSet<Dot>(targetDots);
        HashSet<Edge> edgesToFill = new HashSet<Edge>();
        
        foreach (var edge in edges)
        {
            if (dotSet.Contains(edge.d1) && dotSet.Contains(edge.d2) &&
               (targetEdges.Contains(edge.tag) || targetEdges.Contains(ReverseTag(edge.tag))))
            {
                if (edge.filled)
                {
                    return false;
                }    
                edgesToFill.Add(edge);
            }
        }

        foreach (var edge in edgesToFill)
        {
            edge.filled = true;
            edgesCount--;

            var foundStick = sticks.Find(s => s.currentEdgeTag == edge.tag || s.currentEdgeTag == ReverseTag(edge.tag));
            if (foundStick == null)
            {
                return false;
            }
            edge.stickSprite = foundStick.sprite.gameObject;
        }

        if (edgesCount == 0)
        {
            completed = true;
            SpawnManager.Instance.FillTheCell(transform, fillColor);
            //GridManager.Instance.IsTimeToBlast(this);
            GridManager.Instance.IsTimeToBlast();
        }
        
        return true;
    }
    
    private string ReverseTag(string t)
    {
        var parts = t.Split(',');
        if (parts.Length != 2)
        {
            Debug.LogError("Invalid edge tag: " + t);
            return t;
        }
        return parts[1] + "," + parts[0];
    }
    
    public void SetCoordinates(Vector2Int c)
    {
        coordinates = c;
    }

    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }
    
    public void UpdateState()
    {
        var filledCount = edges.Count(e => e.stickSprite != null);
        completed = filledCount == edges.Count;
        edgesCount = edges.Count - filledCount;

        if (!completed)
        {
            if (transform.childCount > 0)
            {
                fill.SetActive(false);
            }
        }

        foreach (var d in dots)
        {

            if (d.Edges.Exists(e=>e.filled))
            {
                continue;
            }
            d.ResetState();
        }
        
    }

}

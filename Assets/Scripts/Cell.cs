using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] public List<Dot> dots; // representing corners
    [SerializeField] public List<Edge> edges;
    public bool completed;
    
    [SerializeField] private int edgesCount;

    public int id;
    [SerializeField] private Vector2Int coordinates;

    
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
            // Look for another dot with the same x-coordinate (and not the same dot)
            Dot xMatch = dots.Find(n => n != d && n.GetCoordinates().x == d.GetCoordinates().x);
            if (xMatch != null)
            {
                // Check if an edge with these two dots already exists (regardless of order)
                if (!edges.Any(e => (e.d1 == d && e.d2 == xMatch) || (e.d1 == xMatch && e.d2 == d)))
                {
                    edges.Add(new Edge(d, xMatch));
                }
            }

            // Look for another dot with the same y-coordinate
            Dot yMatch = dots.Find(n => n != d && n.GetCoordinates().y == d.GetCoordinates().y);
            if (yMatch != null)
            {
                if (!edges.Any(e => (e.d1 == d && e.d2 == yMatch) || (e.d1 == yMatch && e.d2 == d)))
                {
                    edges.Add(new Edge(d, yMatch));
                }
            }
        }
        
        edgesCount = edges.Count;
    }


    public bool FillEdges(List<Dot> targetDots,HashSet<string> targetEdges,Color fillColor)
    {
        
        HashSet<Dot> dotSet = new HashSet<Dot>(targetDots);
        
        HashSet<Edge> edgesToFill = new HashSet<Edge>();
        
        foreach (var edge in edges)
        {
            if (dotSet.Contains(edge.d1) && dotSet.Contains(edge.d2) && (targetEdges.Contains(edge.tag) || targetEdges.Contains(ReverseTag(edge.tag))))
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
            //edge.d1.BringToFront();
            //edge.d2.BringToFront();
            edgesCount--;
        }

        if (edgesCount == 0)
        {
            completed = true;
            SpawnManager.Instance.FillTheCell(transform,fillColor);
            GridManager.Instance.IsTimeToBlast(this);
        }
        
        return true;
    }

    private string ReverseTag(string t)
    {
        var s = t.Split(',');
        return s[1]+","+s[0];
        
    }
    
    public void SetCoordinates(Vector2Int c)
    {
        coordinates = c;
    }

    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }
    
    public void ResetState(){
        completed = false;
        edgesCount = edges.Count;

        foreach (var edge in edges)
        {
            edge.filled = false;
        }
        
        foreach (var d in dots)
        {
            d.ResetState();
        }
        
    }
    
}

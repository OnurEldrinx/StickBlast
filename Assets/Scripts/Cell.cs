using System.Collections.Generic;
using System.Linq;
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
        // Add this cell to each dot’s list.
        foreach (var dot in dots)
        {
            dot.Cells.Add(this);
        }
        
        DefineEdges();
    }

    /// <summary>
    /// Determines the edges of the cell by connecting the dots that share the same x or y coordinates.
    /// </summary>
    private void DefineEdges()
    {
        edges = new List<Edge>();

        foreach (var d in dots)
        {
            // Look for another dot with the same x-coordinate (but not the same dot).
            Dot xMatch = dots.Find(n => n != d && n.GetCoordinates().x == d.GetCoordinates().x);
            if (xMatch != null)
            {
                // Only add the edge if it isn’t already present.
                if (!edges.Any(e => (e.d1 == d && e.d2 == xMatch) || (e.d1 == xMatch && e.d2 == d)))
                {
                    edges.Add(new Edge(d, xMatch,this));
                }
            }

            // Look for another dot with the same y-coordinate.
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

    /// <summary>
    /// Attempts to fill the cell’s required edges based on the provided target dots and edge tags.
    /// Also assigns the corresponding stick sprite from the shape edges.
    /// </summary>
    /// <param name="targetDots">The dots from the draggable shape.</param>
    /// <param name="targetEdges">A set of edge tags that need to be filled.</param>
    /// <param name="fillColor">The color used to fill the cell.</param>
    /// <param name="sticks">A list of ShapeEdge objects from the draggable piece.</param>
    /// <returns>True if filling is successful; otherwise, false.</returns>
    public bool FillEdges(List<Dot> targetDots, HashSet<string> targetEdges, Color fillColor, List<ShapeEdge> sticks)
    {
        // Update the currentEdgeTag for each shape edge using its snap targets.
        foreach (var shapeEdge in sticks)
        {
            if (shapeEdge.d1.snapTarget != null && shapeEdge.d2.snapTarget != null)
            {
                shapeEdge.currentEdgeTag = shapeEdge.d1.snapTarget.id + "," + shapeEdge.d2.snapTarget.id;
            }
            else
            {
                Debug.LogWarning("One or both snapTargets are null in a ShapeEdge.");
            }
        }
        
        // Convert targetDots to a HashSet for quick lookup.
        HashSet<Dot> dotSet = new HashSet<Dot>(targetDots);
        HashSet<Edge> edgesToFill = new HashSet<Edge>();
        
        // Determine which edges in this cell need to be filled.
        foreach (var edge in edges)
        {
            if (dotSet.Contains(edge.d1) && dotSet.Contains(edge.d2) &&
               (targetEdges.Contains(edge.tag) || targetEdges.Contains(ReverseTag(edge.tag))))
            {
                if (edge.filled)
                {
                    // If any required edge is already filled, the fill fails.
                    return false;
                }    
                edgesToFill.Add(edge);
            }
        }

        // Fill each identified edge.
        foreach (var edge in edgesToFill)
        {
            edge.filled = true;
            edgesCount--;

            // Look for the corresponding shape edge by matching tags.
            var foundStick = sticks.Find(s => s.currentEdgeTag == edge.tag || s.currentEdgeTag == ReverseTag(edge.tag));
            if (foundStick == null)
            {
                Debug.LogError("Could not find matching shape edge for edge tag: " + edge.tag);
                return false;
            }
            edge.stickSprite = foundStick.sprite.gameObject;
        }

        // If all edges have been filled, mark the cell as complete.
        if (edgesCount == 0)
        {
            completed = true;
            SpawnManager.Instance.FillTheCell(transform, fillColor);
            GridManager.Instance.IsTimeToBlast(this);
        }
        
        return true;
    }

    /// <summary>
    /// Returns the reversed version of an edge tag.
    /// Example: "3,5" becomes "5,3".
    /// </summary>
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
    
    public bool IsCompleted()
    {
        return edges.TrueForAll(edge => edge != null); // Adjust this logic based on your game mechanics
    }
    
    /// <summary>
    /// Updates the cell’s state after edges have been (or not) filled.
    /// It recalculates the number of unfilled edges, updates the complete flag, and triggers animations.
    /// </summary>
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

    public void Clear()
    {
        completed = false;
        edgesCount = edges.Count;
    }

}

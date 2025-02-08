using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Edge
{
    public Dot d1;
    public Dot d2;
    public bool filled;
    public string tag;
    public GameObject stickSprite;

    public Cell cell;

    /// <summary>
    /// Constructs an edge connecting two distinct dots.
    /// The dots are ordered by their ID so that d1 always has the lower ID.
    /// The edge is then added to each dot’s list of edges.
    /// </summary>
    /// <param name="dot1">First dot</param>
    /// <param name="dot2">Second dot</param>
    /// <param name="cell"></param>
    public Edge(Dot dot1, Dot dot2,Cell cell)
    {
        // Ensure that the edge is between two different dots.
        if (dot1 == dot2)
        {
            Debug.LogError("Cannot create an edge with the same dot.");
            return;
        }
        
        // Order the dots by their ID.
        if (dot1.id < dot2.id)
        {
            d1 = dot1;
            d2 = dot2;
        }
        else
        {
            d1 = dot2;
            d2 = dot1;
        }
        
        // Add this edge to each dot's list of edges.
        d1.Edges.Add(this);
        d2.Edges.Add(this);

        // Create a unique tag based on the two dot IDs.
        tag = $"{d1.id},{d2.id}";
        
        this.cell = cell;
        
    }
    
    /// <summary>
    /// Attempts to fill the edge using the provided two dots.
    /// If the two dots match the endpoints of this edge (in any order),
    /// then the edge is marked as filled.
    /// </summary>
    /// <param name="dotA">First dot</param>
    /// <param name="dotB">Second dot</param>
    public void TryFill(Dot dotA, Dot dotB)
    {
        if (filled)
            return;
        
        // Check if the provided dots match the endpoints of this edge.
        if ((dotA.id == d1.id && dotB.id == d2.id) ||
            (dotA.id == d2.id && dotB.id == d1.id))
        {
            filled = true;
        }
    }

    public void OnBlast(Cell other)
    {
        
        var neighborsCell = GridManager.Instance.NeighborsOf(other);

        foreach (var neighbor in neighborsCell)
        {
            if (neighbor.edges.Exists(x=>x.tag == tag) && neighbor.completed && !neighbor.edges.Contains(this))
            {
                return;
            }

            var e = neighbor.edges.FirstOrDefault(e => e.tag == tag);
            if (e != null)
            {
                e.filled = false;
                e.stickSprite?.SetActive(false);
                e.stickSprite = null;
            }
        }
        
        filled = false;
        stickSprite?.SetActive(false);
        stickSprite = null;
    }
    
}
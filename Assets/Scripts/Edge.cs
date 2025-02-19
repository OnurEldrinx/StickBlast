using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Edge
{
    public Dot d1;
    public Dot d2;
    public bool filled;
    //public string tag;
    public EdgeKey edgeKey;
    public GameObject stickSprite;

    
    public Edge(Dot dot1, Dot dot2,Cell cell)
    {
        if (dot1 == dot2)
        {
            Debug.LogError("Cannot create an edge with the same dot.");
            return;
        }
        
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
        
        d1.Edges.Add(this);
        d2.Edges.Add(this);

        //tag = $"{d1.id},{d2.id}";
        edgeKey = new EdgeKey(d1.id, d2.id);
    }
    
    public void TryFill(Dot dotA, Dot dotB)
    {
        if (filled)
            return;
        
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
            if (neighbor.edges.Exists(x=>x.edgeKey.Equals(edgeKey)) && neighbor.completed && !neighbor.edges.Contains(this))
            {
                return;
            }

            var e = neighbor.edges.FirstOrDefault(e => e.edgeKey.Equals(edgeKey));
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
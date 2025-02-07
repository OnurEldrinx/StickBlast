using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private Transform gridTransform;
    [SerializeField] private List<Dot> dots;
    [SerializeField] private List<Cell> cells;
    private Dot[,] _dotMatrix;
    
    
    
    private void Awake()
    {
        //dots = FindObjectsByType<Dot>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).ToList();
        dots = gridTransform.GetComponentsInChildren<Dot>().ToList();
        cells = gridTransform.GetComponentsInChildren<Cell>().ToList();
        
        // Generate grid matrix
        int n = (int)Mathf.Sqrt(dots.Count);
        _dotMatrix = new Dot[n, n];
        int dotIndex=0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                var currentDot = dots[dotIndex++];
                currentDot.id = dotIndex;
                _dotMatrix[i, j] = currentDot;
                _dotMatrix[i,j].SetCoordinates(new Vector2Int(i,j));
            }
        }

        //Set Neighbors of Dots
        foreach (var d in dots)
        {
            d.neighbors = NeighborsOf(d);
        }
        
        //Set Cells
        foreach (var c in cells)
        {
            c.dots = NearestFourDots(c.transform.position);
        }
        
    }
    
    private int CompareByNames(Dot d1, Dot d2)
    {
        return string.CompareOrdinal(d1.name, d2.name);
    }

    public Dot NearestDot(Vector2 position)
    {
        return dots
            .OrderBy(d => Vector2.Distance(d.transform.position, position))
            .FirstOrDefault();
    }

    private List<Dot> NearestFourDots(Vector2 position)
    {
        return dots
            .OrderBy(d => Vector2.Distance(d.transform.position, position))
            .Take(4)
            .ToList();
    }
    
    private List<Dot> NeighborsOf(Dot dot)
    {
        int x = dot.GetCoordinates().x;
        int y = dot.GetCoordinates().y;

        var result = new List<Dot>();

        if (x - 1 >= 0)
        {
            result.Add(_dotMatrix[x - 1, y]);
        }

        if (x + 1 < _dotMatrix.GetLength(0))
        {
            result.Add(_dotMatrix[x + 1, y]);
        }

        if (y - 1 >= 0)
        {
            result.Add(_dotMatrix[x, y - 1]);
        }

        if (y + 1 < _dotMatrix.GetLength(1))
        {
            result.Add(_dotMatrix[x, y + 1]);
        }
        
        return result;
    }

    public Edge GetEdge(Dot dot,string t)
    {
        return dot.Edges.FirstOrDefault(e => e.tag == t);
    }

    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private Transform gridTransform;
    [SerializeField] private List<Dot> dots;
    [SerializeField] private List<Cell> cells;
    private Dot[,] _dotMatrix;
    private Cell[,] _cellMatrix;
    
    public List<Cell> blastRow;
    public List<Cell> blastColumn;

    
    public ThemeData themeData;
    
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
        
        //Generate Cell Matrix
        int k = (int)Mathf.Sqrt(cells.Count);
        _cellMatrix = new Cell[k, k];
        int cellIndex=0;
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++)
            {
                var currentCell = cells[cellIndex++];
                currentCell.id = cellIndex;
                _cellMatrix[i,j] = currentCell;
                _cellMatrix[i,j].SetCoordinates(new Vector2Int(i,j));
            }
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
    
    public List<Cell> NeighborsOf(Cell cell)
    {
        int x = cell.GetCoordinates().x;
        int y = cell.GetCoordinates().y;

        var result = new List<Cell>();

        if (x - 1 >= 0)
        {
            result.Add(_cellMatrix[x - 1, y]);
        }

        if (x + 1 < _cellMatrix.GetLength(0))
        {
            result.Add(_cellMatrix[x + 1, y]);
        }

        if (y - 1 >= 0)
        {
            result.Add(_cellMatrix[x, y - 1]);
        }

        if (y + 1 < _cellMatrix.GetLength(1))
        {
            result.Add(_cellMatrix[x, y + 1]);
        }
        
        return result;
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

    public Cell GetCell(Vector2 position)
    {
        return cells.Find(e => Mathf.Approximately(e.transform.position.x, position.x) && Mathf.Approximately(e.transform.position.y, position.y));
    }
    
    public bool IsRowComplete(Cell queryCell)
    {
        var row = cells.FindAll(c=>c.GetCoordinates().x == queryCell.GetCoordinates().x);

        foreach (var c in row)
        {
            if (!c.completed)
            {
                return false;
            }
        }
        
        blastRow = row;
        //print($"Row complete {queryCell.gameObject.name}");
        return true;
        
    }

    public bool IsColumnComplete(Cell queryCell)
    {
        var column = cells.FindAll(c=>c.GetCoordinates().y == queryCell.GetCoordinates().y);

        foreach (var c in column)
        {
            if (!c.completed)
            {
                return false;
            }
        }
        
        blastColumn = column;
        //print($"Column complete {queryCell.gameObject.name}");
        return true;
        
    }

    public async void IsTimeToBlast(Cell queryCell)
    {
        
        try
        {
            //await Task.Delay(250);
        
            HashSet<Cell> neighbors = new HashSet<Cell>();
            
            
            if (IsRowComplete(queryCell))
            {
                //print($"Blasting Row-{queryCell.GetCoordinates().x}!");
                var pos = new Vector3(0, queryCell.transform.position.y,0);
                SpawnManager.Instance.SpawnBlastEffect(pos,false);

                foreach (var c in blastRow)
                {
                    
                    await Task.Delay(50);
                    c.fill.SetActive(false);
                    foreach (var e in c.edges)
                    {
                        e.OnBlast(c);
                    }
                    
                    c.UpdateState();
                    neighbors.UnionWith(NeighborsOf(c));
                    
                }

                await Task.Delay(100);

            }
            
            
            if (IsColumnComplete(queryCell))
            {
                //print($"Blasting Column-{queryCell.GetCoordinates().y}!");
                var pos = new Vector3(queryCell.transform.position.x, 0,0);
                SpawnManager.Instance.SpawnBlastEffect(pos,true);

                foreach (var c in blastColumn)
                {
                    await Task.Delay(50);
                    c.fill.SetActive(false);
                    
                    foreach (var e in c.edges)
                    {
                        e.OnBlast(c);
                    }
                    
                    c.UpdateState();
                    neighbors.UnionWith(NeighborsOf(c));

                }
            }

            //await Task.Delay(75);
            foreach (var n in neighbors)
            {
                n.UpdateState();
            }


        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }
    
    public List<Cell> FindCellByEdge(string t){
        
        return cells.FindAll(c=>c.edges.Any(e => e.tag == t));
        
    }
    
}

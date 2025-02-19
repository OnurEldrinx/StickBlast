using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private Transform gridTransform;
    [SerializeField] public List<Dot> dots;
    [SerializeField] private List<Cell> cells;
    private Dot[,] _dotMatrix;
    private Cell[,] _cellMatrix;

    private readonly List<List<Cell>> _blastRows = new();
    private readonly List<List<Cell>> _blastColumns = new();
    
    public bool blastOnProcess;
    
    public int gridSize;
    
    private void Awake()
    {
        gridTransform = GameObject.Find("Grid").transform;
        dots = gridTransform.GetComponentsInChildren<Dot>().ToList();
        cells = gridTransform.GetComponentsInChildren<Cell>().ToList();
        
        // Generate grid matrix
        int n = (int)Mathf.Sqrt(dots.Count);
        gridSize = n;
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

    private void Start()
    {
        CandidateDots();
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

    public Edge GetEdge(Dot dot,EdgeKey t)
    {
        return dot.Edges.FirstOrDefault(e => e.edgeKey.Equals(t));
    }

    public Cell GetCell(Vector2 position)
    {
        return cells.Find(e => Mathf.Approximately(e.transform.position.x, position.x) && Mathf.Approximately(e.transform.position.y, position.y));
    }

    private List<Cell> GetRowOf(Cell queryCell)
    {
        return cells.FindAll(c=>c.GetCoordinates().x == queryCell.GetCoordinates().x);
    }

    private List<Cell> GetColumnOf(Cell queryCell)
    {
        return cells.FindAll(c=>c.GetCoordinates().y == queryCell.GetCoordinates().y);
    }

    private bool IsRowComplete(List<Cell> row)
    {
        //var row = GetRowOf(queryCell);

        foreach (var c in row)
        {
            if (!c.completed)
            {
                return false;
            }
        }
        
        //blastRows.Add(row);
        //print($"Row complete {queryCell.gameObject.name}");
        return true;
        
    }

    private bool IsColumnComplete(List<Cell> column)
    {
        //var column = GetColumnOf(queryCell);

        foreach (var c in column)
        {
            if (!c.completed)
            {
                return false;
            }
        }
        
        //blastColumns.Add(column);
        //print($"Column complete {queryCell.gameObject.name}");
        return true;
        
    }

    public void IsTimeToBlast()
    {
        StartCoroutine(nameof(CheckBlast));
    }

    public IEnumerator CheckBlast()
    {
        if (blastOnProcess)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.25f);
        
        blastOnProcess = true;
        
        HashSet<Cell> neighbors = new HashSet<Cell>();

        _blastColumns.Clear();
        _blastRows.Clear();
        
        for(var i=0; i<_cellMatrix.GetLength(0);i++)
        {
            var row = GetRowOf(_cellMatrix[i,0]);
            if (IsRowComplete(row))
            {
                _blastRows.Add(row);
            }
        }
        
        for(var i=0; i<_cellMatrix.GetLength(1);i++)
        {
            var column = GetColumnOf(_cellMatrix[0,i]);
            if (IsColumnComplete(column))
            {
                _blastColumns.Add(column);
            }
        }

        foreach (var row in _blastRows)
        {
            foreach (var c in row)
            {
                //await Task.Delay(50);
                //yield return new WaitForSeconds(0.025f);
                c.fill.SetActive(false);
                
                foreach (var e in c.edges)
                {
                    e.OnBlast(c);
                }
                
                c.UpdateState();
                neighbors.UnionWith(NeighborsOf(c));
            }
            
            //Blast Effect For Each Blast Row
            var pos = new Vector3(0, row.First().transform.position.y,0);
            SpawnManager.Instance.SpawnBlastEffect(pos,false);

        }

        foreach (var column in _blastColumns)
        {
            foreach (var c in column)
            {
                //await Task.Delay(50);
                //yield return new WaitForSeconds(0.025f);
                c.fill.SetActive(false);
                    
                foreach (var e in c.edges)
                {
                    e.OnBlast(c);
                }
                
                c.UpdateState();
                neighbors.UnionWith(NeighborsOf(c));
            }
            
            //Blast Effect For Each Blast Column
            var pos = new Vector3(column.First().transform.position.x, 0,0);
            SpawnManager.Instance.SpawnBlastEffect(pos,true);
        }
        
        foreach (var n in neighbors)
        {
            n.UpdateState();
        }
        
        blastOnProcess = false;
        
        
    }
    
    public List<Dot> CandidateDots()
    {
        return dots.FindAll(d=>d.Edges.Count(e=>!e.filled) > 0);
    }
    
    public Dot GetDotWithOffset(Dot from,Vector2Int offset)
    {
        return dots.Find(d => from.GetCoordinates() + offset == d.GetCoordinates());
    }
    

}

using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GridFrameScaler frameScaler;
    [SerializeField] private SpriteRenderer gridLine;
    [SerializeField] private Dot dot;
    [SerializeField] private Cell cell;
    
    [SerializeField] private Vector2Int gridSize;

    private void Awake()
    {
        Generate();
    }

    public void Generate()
    {

        frameScaler.AdjustScale(gridSize.x, gridSize.y);
        
        GameObject grid = new GameObject("Grid");
        
        int dotCountX = gridSize.x + 1;
        int dotCountY = gridSize.y + 1;
        float startPosX = -gridSize.x / 2f;
        float startPosY = gridSize.y / 2f;
        int index = 0;
        
        GameObject dotsParent = new GameObject("Dots");
        GameObject linesParent = new GameObject("GridLines");
        for (int i=0; i<dotCountY; i++)
        {
            for (int j = 0; j < dotCountX; j++)
            {
                Dot d = Instantiate(dot,dotsParent.transform);
                d.gameObject.name = $"Dot({index++})";
                d.transform.position = new Vector2(startPosX + j, startPosY - i);
                d.id = index;

                if (j < dotCountX-1)
                {
                    var line = Instantiate(gridLine).transform;
                    line.transform.position = d.transform.position + new Vector3(0.5f,0f,0f);
                    line.transform.parent = linesParent.transform;
                }
                
                if (i < dotCountY-1)
                {
                    var line = Instantiate(gridLine).transform;
                    line.transform.position = d.transform.position + new Vector3(0f,-0.5f,0f);
                    line.transform.rotation = Quaternion.Euler(0f,0f,90f);
                    line.transform.parent = linesParent.transform;
                }
                
            }
            
            
            
            
        }
        
        
        startPosX += 0.5f;
        startPosY -= 0.5f;
        index = 0;
        
        GameObject cellsParent = new GameObject("Cells");
        for (int i = 0; i < gridSize.y; i++)
        {
            for (int j = 0; j < gridSize.x; j++)
            {
                var c = Instantiate(cell,cellsParent.transform);
                c.gameObject.name = $"Cell({index++})";
                c.transform.position = new Vector2(startPosX + j, startPosY - i);
                c.id = index;
            }
        }
        
        dotsParent.transform.parent = grid.transform;
        cellsParent.transform.parent = grid.transform;
        
        CameraSizeScaler.OnAdjust.Invoke(gridSize.x, gridSize.y);
        
    }
}

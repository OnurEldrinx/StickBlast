using UnityEngine;

public class GridFrameScaler : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private Transform right;
    [SerializeField] private Transform left;
    
    public void AdjustScale(int gridWidth, int gridHeight)
    {
        gridWidth = gridWidth < 5 ? 5 : gridWidth;
        gridHeight = gridHeight < 5 ? 5 : gridHeight;
        
        var x = gridWidth+0.5f;
        var y = gridHeight+0.5f;
        
        center.localScale = new Vector3(x, y, 1);
        right.localScale = new Vector3(x, y, 1);
        left.localScale = new Vector3(x, y, 1);
        top.localScale = new Vector3(x, y, 1);
        bottom.localScale = new Vector3(x, y, 1);
        
        center.localPosition = Vector3.zero;
        right.localPosition = new Vector3(center.localScale.x/2f, 0, 0);
        left.localPosition = new Vector3(-center.localScale.x/2f, 0, 0);
        top.localPosition = new Vector3(0, center.localScale.y/2f, 0);
        bottom.localPosition = new Vector3(0, -center.localScale.y/2f, 0);
        
    }
}

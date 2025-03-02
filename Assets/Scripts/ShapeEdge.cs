using System;
using UnityEngine;

[Serializable]
public class ShapeEdge
{
    public SpriteRenderer sprite;
    public DotSensor d1;
    public DotSensor d2;
    
    public EdgeKey currentEdgeTag;
    
    public ShapeEdge(DotSensor d1, DotSensor d2)
    {
        this.d1 = d1;
        this.d2 = d2;
    }
    
    public void ResetEdge()
    {
        if (sprite != null)
        {
            sprite.gameObject.SetActive(true);
        }
        
        if (d1 != null)
        {
            d1.ResetSensor();
        }
        
        if (d2 != null)
        {
            d2.ResetSensor();
        }

        currentEdgeTag = new EdgeKey(-1, -1);
    }
}
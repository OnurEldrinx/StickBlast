using System;

[Serializable]
public class ShapeEdge
{
    public DotSensor d1;
    public DotSensor d2;
    
    // Used for draggable
    public ShapeEdge(DotSensor d1, DotSensor d2)
    {
        this.d1 = d1;
        this.d2 = d2;
    }
}

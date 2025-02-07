using System;

[Serializable]
public class Edge
{
    public Dot d1;
    public Dot d2;
    public bool filled;
    public string tag;
    
    public Edge(Dot dot1, Dot dot2)
    {
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

        tag = $"{d1.id},{d2.id}";

    }
    
    
    public void TryFill(Dot d, Dot e)
    {
        if (filled)return;
        
        var counter = 0;
        if (d.id == d1.id || d.id == d2.id)
        {
            counter++;
        }

        if (e.id == d1.id || e.id == d2.id)
        {
            counter++;
        }

        if (counter == 2)
        {
            filled = true;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private List<Dot> dots;

    private void Awake()
    {
        dots = FindObjectsByType<Dot>(FindObjectsInactive.Exclude,FindObjectsSortMode.InstanceID).ToList();
    }

    public Dot NearestDot(Vector2 position)
    {
        return dots
            .OrderBy(d => Vector2.Distance(d.transform.position, position))
            .FirstOrDefault();
    }

    

    
}

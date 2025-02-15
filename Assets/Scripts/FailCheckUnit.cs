using System.Collections.Generic;
using UnityEngine;

public class FailCheckUnit : MonoBehaviour
{
    public int id;
    public DotSensor[] dotSensors;
    public Transform anchorSensor;
    
    private void Awake()
    {
        dotSensors = GetComponentsInChildren<DotSensor>();
        anchorSensor = transform.GetChild(0);
        
    }

    public bool IsCandidatePlaceAvailable()
    {
        var snapTargets = new List<Dot>();

        foreach (var sensor in dotSensors)
        {
            if (!sensor.snapTarget)
            {
                return false;
            } 
            snapTargets.Add(sensor.snapTarget);
        }

        for (int i = 0; i < snapTargets.Count - 1; i++)
        {
            Dot current = snapTargets[i];
            Dot next = snapTargets[i + 1];

            var edge = GridManager.Instance.GetEdge(current, current.id + "," + next.id);
            var edgeAlternative = GridManager.Instance.GetEdge(current, next.id + "," + current.id);

            if (edge is { filled: true } || edgeAlternative is { filled: true })
            {
                return false;
            }
        }
        return true;
    }
}

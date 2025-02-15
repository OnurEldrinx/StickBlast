using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FailCheckManager : Singleton<FailCheckManager>
{
    [SerializeField] private List<FailCheckUnit> units;
    [SerializeField] private Dot[] dots;
    public UnityEvent onFail;
    
    private void Start()
    {
        units = FindObjectsByType<FailCheckUnit>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).ToList();
        dots = GridManager.Instance.dots.ToArray();
    }

    public void CheckFail(List<Draggable> piecesInTray)
    {
        StartCoroutine(IsFailed(piecesInTray));
    }

    private IEnumerator IsFailed(List<Draggable> piecesInTray)
    {
        bool failed = true;
        
        var pieces = new List<Draggable>(piecesInTray);
        foreach (var piece in pieces)
        {
            var targetUnit = units.Find(u=>u.id == piece.data.id);
            foreach (var dot in GridManager.Instance.CandidateDots())
            {

                yield return new WaitForSeconds(0.05f);
                targetUnit.anchorSensor.position = dot.transform.position;
                var sensors = new List<DotSensor>(targetUnit.dotSensors).Where(s=>s.localCoordinate != Vector2.zero);
                foreach (var dotSensor in sensors)
                {
                    var next = dot.GetDotWithOffset(dotSensor.localCoordinate);
                    if (!next)
                    {
                        break;
                    }
                    dotSensor.transform.position =next.transform.position;
                }

                if (targetUnit.IsCandidatePlaceAvailable())
                {
                    failed = false;
                    break;
                }
            }

            if (!failed)
            {
                break;
            }
        }


        if (failed)
        {
            print("FAILED!");
            onFail.Invoke();
        }
        else
        {
            print("NO FAIL!");
        }
    }
    
    
}

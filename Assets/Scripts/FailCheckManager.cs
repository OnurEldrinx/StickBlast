using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

public class FailCheckManager : Singleton<FailCheckManager>
{
    public UnityEvent onFail;
    
    public async void CheckFailCondition(List<Draggable> piecesInTray)
    {
        try
        {
            await Task.Delay(500);
        
            var pieces = new List<Draggable>(piecesInTray);
        
            bool failed = true;
        
            foreach (var d in pieces)
            {
                foreach (var candidateDot in GridManager.Instance.CandidateDots())
                {
                    if (d.CanPlaceOnGrid(candidateDot))
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
                onFail.Invoke();
            }
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }
    
    
}

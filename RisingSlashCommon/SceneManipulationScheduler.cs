using System.Collections.Generic;
using HarmonyLib;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class SceneManipulationScheduler
{
    public static SceneManipulationScheduler MainScheduler;
    private List<SceneManipulationRequest> requests = new List<SceneManipulationRequest>();
    public class SceneManipulationRequest
    {
        public object RequestSource = null;
        public int Priority = 0;
        public bool IsComplete = false;

        public SceneManipulationRequest(object source)
        {
            RequestSource = source;
        }
        
        public SceneManipulationRequest(object source, int priority, bool isComplete)
        {
            RequestSource = source;
            Priority = priority;
            IsComplete = isComplete;
        }
    }
    
    public SceneManipulationRequest GetRequestBySource(object source)
    {
        SceneManipulationRequest req = null;
        var ind = IndexOfRequestBySource(source);
        if (ind > -1)
        {
            req = requests[ind];
        }

        return req;
    }
    
    public int IndexOfRequestBySource(object source)
    {
        var requestIndex = -1;
        for (int i = 0; i < requests.Count; i++)
        {
            if (requests[i].RequestSource == source)
            {
                requestIndex = i;
                break;
            }
        }

        return requestIndex;
    }
    
    public bool IsHighestPriority(SceneManipulationRequest req)
    {
        var isHighest = true;
        for (int i = 0; i < requests.Count; i++)
        {
            if ( requests[i] != req
                && requests[i].Priority >= req.Priority)
            {
                isHighest = false;
                break;
            }
        }

        return isHighest;
    }

    public bool IsTopRequest(SceneManipulationRequest req)
    {
        return requests.Count > 0 && requests[0] == req;
    }

    public bool ContainsRequests(object source)
    {
        return (IndexOfRequestBySource(source) > -1);
    }

    public bool RequestManipulateScene(object source, int priority)
    {
        var canProceed = false;
        var req = GetRequestBySource(source);
        if (req == null)
        {
            // Intentionally return false on first request to give other objects an opportunity to request on the same frame. We only grant *after* they're in
            req = new SceneManipulationRequest(source, priority, false);
            requests.Add(req);
        }
        else
        {
            if (IsHighestPriority(req))
            {
                canProceed = true;
            }
            else if (IsTopRequest(req))
            {
                canProceed = true;
            }
        }
        return canProceed;
    }
    
    public bool RequestComplete(object source)
    {
        var completed = true;
        var indToRemove = IndexOfRequestBySource(source);
        if (indToRemove > -1)
        {
            requests.RemoveAt(indToRemove);
        }
        else
        {
            completed = false;
        }

        return completed;
    }
}
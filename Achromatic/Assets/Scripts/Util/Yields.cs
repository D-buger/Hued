using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Yields
{
    private static WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    public static WaitForEndOfFrame EndOfFrame{
        get { return endOfFrame; }
    }

    private static WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
    public static WaitForFixedUpdate FixedUpdate
    {
        get { return fixedUpdate; }
    }

    private static Dictionary<float, WaitForSeconds> cashingTime = new Dictionary<float, WaitForSeconds>();
    public static WaitForSeconds WaitSeconds(float seconds)
    {
        if(!cashingTime.ContainsKey(seconds))
        {
            cashingTime.Add(seconds, new WaitForSeconds(seconds));
        }
        return cashingTime[seconds];
    }
}

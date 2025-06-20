using System.Collections.Generic;
using UnityEngine;

public static class YieldInstance
{
    private static Dictionary<float, WaitForSeconds> _waitForSecondsMap = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (_waitForSecondsMap.TryGetValue(seconds, out var instance))
        {
            return instance;
        }
        else
        {
            var newInstance = new WaitForSeconds(seconds);
            _waitForSecondsMap.Add(seconds, newInstance);
            return newInstance;
        }
    }
}

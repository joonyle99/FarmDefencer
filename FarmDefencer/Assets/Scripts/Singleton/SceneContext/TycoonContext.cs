using System;
using UnityEngine;

public class TycoonContext : SceneContext
{
    public static new TycoonContext Current
    {
        get
        {
            var current = Instance as TycoonContext;

            if (current == null)
            {
                throw new NullReferenceException("The TycoonContext Instance is null.");
            }

            return current;
        }
    }

    // tycoon reference

}

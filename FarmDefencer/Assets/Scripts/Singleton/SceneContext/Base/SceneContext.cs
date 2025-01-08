using Com.LuisPedroFonseca.ProCamera2D;
using System;
using UnityEngine;

public class SceneContext : JoonyleGameDevKit.Singleton<SceneContext>
{
    public static SceneContext Current
    {
        get
        {
            if (Instance == null)
            {
                throw new NullReferenceException("The SceneContext Instance is null.");
            }

            return Instance;
        }
    }

    // comman reference
    public ProCamera2D ProCamera;
}

using System;
using UnityEngine;

public class DefenceContext : SceneContext
{
    public static new DefenceContext Current
    {
        get
        {
            var current = Instance as DefenceContext;

            if (current == null)
            {
                throw new NullReferenceException("The DefenceContext Instance is null.");
            }

            return current;
        }
    }

    // defence reference
    public GridMap GridMap { get; private set; }
    public WaveSystem WaveSystem { get; private set; }
    public TowerBuildSystem BuildSystem { get; private set; }
}

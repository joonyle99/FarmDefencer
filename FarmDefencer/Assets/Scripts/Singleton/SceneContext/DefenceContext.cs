using System;
using UnityEngine;

public class DefenceContext : SceneContext
{
    public static new DefenceContext Current
    {
        get
        {
            var current = Instance as DefenceContext;

            //if (current == null)
            //{
            //    throw new NullReferenceException("The DefenceContext Instance is null.");
            //}

            return current;
        }
    }

    // defence reference
    public GridMap GridMap;
    public WaveSystem WaveSystem;
    public TowerBuildSystem TowerBuildSystem;

    protected override void Awake()
    {
        base.Awake();

        GridMap = FindFirstObjectByType<GridMap>();
        WaveSystem = FindFirstObjectByType<WaveSystem>();
        TowerBuildSystem = FindFirstObjectByType<TowerBuildSystem>();

        if (GridMap != null && WaveSystem != null && TowerBuildSystem != null)
        {
            string log = "";

            log += "DefenceContext is ready.\n";
            //log += $"============================\n";
            log += $"{nameof(GridMap)} is ready.\n";
            log += $"{nameof(WaveSystem)} is ready.\n";
            log += $"{nameof(TowerBuildSystem)} is ready.\n";

            Debug.Log(log);
        }
        else
        {
            Debug.LogError("DefenceContext is not ready.");
        }
    }
    protected override void Start()
    {
        base.Start();

        GameStateManager.Instance.ChangeState(GameState.Build);
    }
}

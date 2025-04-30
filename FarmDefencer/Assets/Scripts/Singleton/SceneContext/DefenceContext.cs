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
    public BuildSystem BuildSystem;
    public WaveSystem WaveSystem;

    protected override void Awake()
    {
        base.Awake();

        GridMap = FindFirstObjectByType<GridMap>();
        BuildSystem = FindFirstObjectByType<BuildSystem>();
        WaveSystem = FindFirstObjectByType<WaveSystem>();

        if (GridMap != null && BuildSystem != null && WaveSystem != null)
        {
            string log = "";

            log += "DefenceContext is ready.\n";
            //log += $"============================\n";
            log += $"{nameof(GridMap)} is ready.\n";
            log += $"{nameof(BuildSystem)} is ready.\n";
            log += $"{nameof(WaveSystem)} is ready.\n";

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

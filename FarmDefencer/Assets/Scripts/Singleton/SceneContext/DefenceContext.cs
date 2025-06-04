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

    // background video
    private VideoController _videoController;

    // defence reference
    public GridMap GridMap;
    public BuildSystem BuildSystem;
    public WaveSystem WaveSystem;
    public BuildUI BuildUI;
    public UpgradeUI UpgradeUI;

    protected override void Awake()
    {
        base.Awake();

        _videoController = GetComponent<VideoController>();

        GridMap = FindFirstObjectByType<GridMap>();
        BuildSystem = FindFirstObjectByType<BuildSystem>();
        WaveSystem = FindFirstObjectByType<WaveSystem>();
        BuildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        UpgradeUI = FindFirstObjectByType<UpgradeUI>(FindObjectsInactive.Include);

        if (GridMap != null && BuildSystem != null && WaveSystem != null)
        {
            string log = "";

            log += "DefenceContext is ready.\n";
            //log += $"============================\n";
            log += $"{nameof(GridMap)} is ready.\n";
            log += $"{nameof(BuildSystem)} is ready.\n";
            log += $"{nameof(WaveSystem)} is ready.\n";
            log += $"{nameof(BuildUI)} is ready.\n";
            log += $"{nameof(UpgradeUI)} is ready.\n";

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

        // 맵을 로드하고 변경될 때마다 배경 비디오를 갱신
        MapManager.Instance.OnMapChanged -= BackgroundVideoHandler;
        MapManager.Instance.OnMapChanged += BackgroundVideoHandler;
        MapManager.Instance.LoadCurrentMap();

        GameStateManager.Instance.ChangeState(GameState.Build);
    }

    public void BackgroundVideoHandler(MapEntry map)
    {
        Debug.Log("Background Video Handler: " + map.name);

        _videoController.StopVideo();
        _videoController.PlayVideo(map.MapId);
    }
}

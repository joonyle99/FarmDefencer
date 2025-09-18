using UnityEngine;

public class DefenceContext : MonoBehaviour
{
    public static DefenceContext Current { get; private set; }

    // background video
    private VideoController _videoController;

    // defence reference
    [SerializeField] private GridMap gridMap;
    public GridMap GridMap => gridMap;
    [SerializeField] private BuildSystem buildSystem;
    public BuildSystem BuildSystem => buildSystem;
    [SerializeField] private WaveSystem waveSystem;
    public WaveSystem WaveSystem => waveSystem;
    [SerializeField] private DefenceUIController defenceUIController;
    public DefenceUIController DefenceUIController => defenceUIController;

    private void Awake()
    {
        Current = this;

        _videoController = GetComponent<VideoController>();
        if (_videoController == null)
        {
            Debug.LogError("VideoController is not assigned in DefenceContext.");
        }

        if (GridMap != null && BuildSystem != null && WaveSystem != null && DefenceUIController != null)
        {
            string log = "";

            log += $"<color=green>DefenceContext is ready.</color>\n";
            //log += $"============================\n";
            log += $"{nameof(GridMap)} is ready.\n";
            log += $"{nameof(BuildSystem)} is ready.\n";
            log += $"{nameof(WaveSystem)} is ready.\n";
            log += $"{nameof(DefenceUIController)} is ready.\n";

            Debug.Log(log);
        }
        else
        {
            Debug.LogError("DefenceContext is not ready.");
        }
    }
    private void Start()
    {
        SoundManager.Instance.PlayDefenceAmb(MapManager.Instance.CurrentMap);
        SoundManager.Instance.PlayDefenceBgm(MapManager.Instance.CurrentMap);
    }

    private void OnEnable()
    {
        if (MapManager.Instance is not null)
        {
            MapManager.Instance.OnMapChanged += BackgroundHandler;
        }
    }

    private void OnDisable()
    {
        if (MapManager.Instance is not null)
        {
            MapManager.Instance.OnMapChanged -= BackgroundHandler;
        }
    }

    private void BackgroundHandler(MapEntry map)
    {
        _videoController.PlayVideo(map.MapCode);
    }
}

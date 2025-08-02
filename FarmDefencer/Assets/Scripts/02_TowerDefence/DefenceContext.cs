using System;
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
    [SerializeField] private BuildUI buildUI;
    public BuildUI BuildUI => buildUI;
    [SerializeField] private UpgradeUI upgradeUI;
    public UpgradeUI UpgradeUI => upgradeUI;

    private void Awake()
    {
        Current = this;
        _videoController = GetComponent<VideoController>();

        if (GridMap != null && BuildSystem != null && WaveSystem != null)
        {
            string log = "";

            log += $"<color=green>DefenceContext is ready.</color>\n";
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

    private void OnEnable()
    {
        MapManager.Instance.OnMapChanged += BackgroundVideoHandler;
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance is not null)
        {
            MapManager.Instance.OnMapChanged -= BackgroundVideoHandler;
        }
    }

    private void BackgroundVideoHandler(MapEntry map)
    {
        //Debug.Log("Background Video Handler: " + map.name);

        _videoController.StopVideo();
        _videoController.PlayVideo(map.MapId);
    }
}

using UnityEngine;

public class DefenceContext : MonoBehaviour
{
    public static DefenceContext Current { get; private set; }

    // defence reference
    [SerializeField] private GridMap _gridMap;
    public GridMap GridMap => _gridMap;
    [SerializeField] private BuildSystem _buildSystem;
    public BuildSystem BuildSystem => _buildSystem;
    [SerializeField] private WaveSystem _waveSystem;
    public WaveSystem WaveSystem => _waveSystem;
    [SerializeField] private DefenceUIController _defenceUIController;
    public DefenceUIController DefenceUIController => _defenceUIController;

    private void Awake()
    {
        Current = this;

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
        SoundManager.Instance?.PlayDefenceAmb(MapManager.Instance.CurrentMap);
        SoundManager.Instance?.PlayDefenceBgm(MapManager.Instance.CurrentMap);
    }
}

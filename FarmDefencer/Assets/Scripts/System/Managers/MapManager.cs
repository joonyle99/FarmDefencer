using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>, IFarmSerializable
{
    // map entry
    [SerializeField] private MapEntry[] _mapEntries;
    public int MapCount => _mapEntries.Length;

    // current map
    private int _currentMapIndex = 1;
    public int CurrentMapIndex { get => _currentMapIndex; set => _currentMapIndex = Mathf.Clamp(value, 1, MapCount); }
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(CurrentMapIndex - 1, 0, MapCount - 1)];

    // current stage
    private int _currentStageIndex = 1;
    public int CurrentStageIndex { get => _currentStageIndex; set => _currentStageIndex = Mathf.Max(1, value); }

    // event
    public event Action<MapEntry> OnMapChanged;

    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("CurrentMapIndex", CurrentMapIndex);
        jsonObject.Add("CurrentStageIndex", CurrentStageIndex);
        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        CurrentMapIndex = json.Property("CurrentMapIndex")?.Value.Value<int>() ?? 1;
        CurrentStageIndex = json.Property("CurrentStageIndex")?.Value.Value<int>() ?? 1;
        LoadCurrentMap();
    }

    public void MoveToNextMap()
    {
        if (CurrentMapIndex + 1 >= MapCount)
        {
            Debug.Log("마지막 맵입니다. 더 이상 이동할 수 없습니다.");
            return;
        }

        CurrentMapIndex++;
        LoadCurrentMap();
    }
    public void MoteToPreviousMap()
    {
        if (CurrentMapIndex - 1 < 1)
        {
            Debug.Log("첫 번째 맵입니다. 더 이상 이동할 수 없습니다.");
            return;
        }

        CurrentMapIndex--;
        LoadCurrentMap();
    }

    public void LoadCurrentMap()
    {
        var currentMap = CurrentMap;
        OnMapChanged?.Invoke(currentMap);
    }
}

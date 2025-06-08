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
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(_currentMapIndex-1, 0, MapCount - 1)];   
    
    public int CurrentStage => 2;

    // event
    public event Action<MapEntry> OnMapChanged;

    public JObject Serialize() => new(new JProperty("CurrentMapIndex", _currentMapIndex));

    public void Deserialize(JObject json)
    {
        _currentMapIndex = json.Property("CurrentMapIndex")?.Value.Value<int>() ?? 0;
        LoadCurrentMap();
    }

    public void MoveToNextMap()
    {
        if (_currentMapIndex + 1 >= MapCount)
        {
            Debug.Log("마지막 맵입니다. 더 이상 이동할 수 없습니다.");
            return;
        }

        _currentMapIndex++;
        LoadCurrentMap();
    }
    public void MoteToPreviousMap()
    {
        if (_currentMapIndex - 1 < 1)
        {
            Debug.Log("첫 번째 맵입니다. 더 이상 이동할 수 없습니다.");
            return;
        }

        _currentMapIndex--;
        LoadCurrentMap();
    }

    public void LoadCurrentMap()
    {
        var currentMap = CurrentMap;
        //currentMap.Initialize();
        OnMapChanged?.Invoke(currentMap);
    }
}

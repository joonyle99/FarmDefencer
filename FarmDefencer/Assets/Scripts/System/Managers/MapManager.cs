using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum MAP_CODE
{
    FOREST = 0,
    BEACH = 1,
    CAVE = 2,
}

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>, IFarmSerializable
{
    // map entry
    [SerializeField] private MapEntry[] _mapEntries;
    public int MapCount => _mapEntries.Length;

    // current map
    private int _currentMapIndex = 1;
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(_currentMapIndex-1, 0, MapCount - 1)];   
    
    public int CurrentStage => 2;

    public int CurrentTurn { get; private set; } // 디펜스 성공 유무와 관계없이 디펜스에서 타이쿤으로 복귀할 때마다 IncrementTurn() 하여 1씩 증가하는 수.

    // event
    public event Action<MapEntry> OnMapChanged;

    public JObject Serialize()
    {
        var jObject = new JObject
        {
            { "CurrentMapIndex", _currentMapIndex },
            { "CurrentTurn", CurrentTurn }
        };

        return jObject;
    }

    public void Deserialize(JObject json)
    {
        _currentMapIndex = json.Property("CurrentMapIndex")?.Value.Value<int>() ?? 0;
        CurrentTurn = json.Property("CurrentTurn")?.Value.Value<int>() ?? 0;
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

    public void IncrementTurn() => CurrentTurn += 1;
}

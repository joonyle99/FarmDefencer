using System;
using UnityEngine;

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>
{
    // map entry
    [SerializeField] private MapEntry[] _mapEntries;
    public int MapCount => _mapEntries.Length;

    // current map
    private int _currentMapIndex = 0;
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(_currentMapIndex, 0, MapCount - 1)];

    // event
    public Action<MapEntry> OnMapChanged;

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
        if (_currentMapIndex - 1 < 0)
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

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>, IFarmSerializable
{
    // map entry
    [SerializeField] private MapEntry[] _mapEntries;
    public int MapCount => _mapEntries.Length;
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(CurrentMapIndex - 1, 0, MapCount - 1)];

    private int _currentMapIndex;
    /// <summary>
    /// 디펜스를 위해 사용. 현재 선택된 맵.
    /// </summary>
    public int CurrentMapIndex
    {
        get => _currentMapIndex;
        set
        {
            if (value <= 0 || value > MaximumUnlockedMapIndex)
            {
                Debug.LogError($"CurrentMapIndex는 0보다 크고 MaximumUnlockedMapIndex({MaximumUnlockedStageIndex})보다 작거나 같아야 합니다.");
            }

            _currentMapIndex = Math.Clamp(value, 1, MaximumUnlockedMapIndex);
            OnMapChanged?.Invoke(CurrentMap);
        }
    }

    private int _currentStageIndex;

    /// <summary>
    /// 디펜스를 위해 사용. 현재 선택된 스테이지.
    /// </summary>
    public int CurrentStageIndex
    {
        get => _currentStageIndex;
        set
        {
            if (value <= 0 || CurrentMapIndex == MaximumUnlockedStageIndex && value > MaximumUnlockedStageIndex)
            {
                Debug.LogError($"CurrentStageIndex는 0보다 크고 CurrentMapIndex == MaximumUnlockedMapIndex인 경우 MaximumUnlockedStageIndex({MaximumUnlockedStageIndex})보다 작거나 같아야 합니다.");
            }

            _currentStageIndex = value;
            if (_currentStageIndex <= 0 ||
                CurrentMapIndex == MaximumUnlockedMapIndex && _currentStageIndex > MaximumUnlockedStageIndex)
            {
                _currentStageIndex = MaximumUnlockedStageIndex;
            }
        }
    }
    
    public int MaximumUnlockedMapIndex { get; private set; }
    public int MaximumUnlockedStageIndex { get; private set; }

    // event
    public event Action<MapEntry> OnMapChanged;

    public MapEntry GetMapEntryOf(int mapIndex) => _mapEntries[mapIndex-1];

    public MapEntry GetMapEntryOf(string mapCode) => _mapEntries.FirstOrDefault(entry => entry.MapCode.Equals(mapCode));

    public void InvokeOnMapChanged() => OnMapChanged?.Invoke(CurrentMap);
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("CurrentMapIndex", CurrentMapIndex);
        jsonObject.Add("CurrentStageIndex", CurrentStageIndex);       
        jsonObject.Add("MaximumUnlockedMapIndex", MaximumUnlockedMapIndex);
        jsonObject.Add("MaximumUnlockedStageIndex", MaximumUnlockedStageIndex);
        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        MaximumUnlockedMapIndex = json.ParsePropertyOrAssign("MaximumUnlockedMapIndex", 1);
        MaximumUnlockedStageIndex = json.ParsePropertyOrAssign("MaximumUnlockedStageIndex", 1);
        CurrentMapIndex = json.ParsePropertyOrAssign("CurrentMapIndex", MaximumUnlockedMapIndex);
        CurrentStageIndex = json.ParsePropertyOrAssign("CurrentStageIndex", MaximumUnlockedStageIndex);
        
        // Current Index <= Maximum Unlocked Index 보정
        if (CurrentMapIndex > MaximumUnlockedMapIndex)
        {
            CurrentMapIndex = MaximumUnlockedMapIndex;
        }

        if (CurrentStageIndex > MaximumUnlockedStageIndex && CurrentMapIndex == MaximumUnlockedMapIndex)
        {
            CurrentStageIndex = MaximumUnlockedStageIndex;    
        }
        
        OnMapChanged?.Invoke(CurrentMap);
    }

    /// <summary>
    /// 디펜스에서 현재 스테이지를 클리어한 모든 상황에서 호출하는 메소드.
    /// 
    /// <remarks>마지막 맵 마지막 스테이지거나, 가장 마지막 스테이지를 클리어한게 아닐 때에 대한 처리를 진행하므로 호출자 코드에서 분기할 필요 없음.</remarks>
    /// </summary>
    public void ClearCurrentStage()
    {
        if (CurrentMapIndex != MaximumUnlockedMapIndex || CurrentStageIndex != MaximumUnlockedStageIndex || // 최신 스테이지를 클리어한게 아니거나
            MaximumUnlockedMapIndex == 3 && MaximumUnlockedStageIndex == 10) // 마지막 스테이지라면 처리할게 없음
        {
            return;
        }

        MaximumUnlockedStageIndex += 1;
        if (MaximumUnlockedStageIndex > 10)
        {
            MaximumUnlockedStageIndex = 1;
            MaximumUnlockedMapIndex += 1;
        }
        
        OnMapChanged?.Invoke(CurrentMap);
    }

    public void Debug_SetCurrentMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= MapCount)
        {
            throw new ArgumentOutOfRangeException(nameof(mapIndex));
        }

        CurrentMapIndex = mapIndex;
        OnMapChanged?.Invoke(CurrentMap);
    }

    protected override void Awake()
    {
        base.Awake();
        
        MaximumUnlockedMapIndex = 1;
        MaximumUnlockedStageIndex = 1;
        CurrentMapIndex = MaximumUnlockedMapIndex;
        CurrentStageIndex = MaximumUnlockedStageIndex;
        
        OnMapChanged?.Invoke(CurrentMap);
    }
    
    // public void MoveToNextMap()
    // {
    //     if (CurrentMapIndex + 1 >= MapCount)
    //     {
    //         Debug.Log("마지막 맵입니다. 더 이상 이동할 수 없습니다.");
    //         return;
    //     }
    //
    //     CurrentMapIndex++;
    //     LoadCurrentMap();
    // }
    // public void MoteToPreviousMap()
    // {
    //     if (CurrentMapIndex - 1 < 1)
    //     {
    //         Debug.Log("첫 번째 맵입니다. 더 이상 이동할 수 없습니다.");
    //         return;
    //     }
    //
    //     CurrentMapIndex--;
    //     LoadCurrentMap();
    // }
}

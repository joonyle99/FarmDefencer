using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>, IFarmSerializable
{
    // map entry
    [SerializeField] private MapEntry[] _mapEntries;
    public int MapCount => _mapEntries.Length;
    public MapEntry CurrentMap => _mapEntries[Mathf.Clamp(CurrentMapIndex - 1, 0, MapCount - 1)];

    private int _currentMapIndex = 1;
    /// <summary>
    /// 현재 선택된 맵
    /// </summary>
    public int CurrentMapIndex
    {
        get => _currentMapIndex;
        set
        {
            if (value < 1 || value > MaximumUnlockedMapIndex)
            {
                Debug.LogError($"CurrentMapIndex는 {1} 이상 {MaximumUnlockedMapIndex} 이하여야 합니다. - value({value})");
            }

            _currentMapIndex = Math.Clamp(value, 1, MaximumUnlockedMapIndex);

            OnMapChanged?.Invoke(CurrentMap);
        }
    }

    private int _currentStageIndex = 1;

    /// <summary>
    /// 현재 선택된 스테이지
    /// </summary>
    public int CurrentStageIndex
    {
        get => _currentStageIndex;
        set
        {
            if (value < 1 || value > MaximumUnlockedStageIndex)
            {
                Debug.LogError($"CurrentStageIndex는 {1} 이상 {MaximumUnlockedStageIndex} 이하여야 합니다. - value({value})");
            }

            _currentStageIndex = Math.Clamp(value, 1, MaximumUnlockedStageIndex);
        }
    }

    public int MaximumUnlockedMapIndex { get; private set; } // 해금된 맵 인덱스
    public int MaximumUnlockedStageIndex { get; private set; } // 해금된 스테이지 인덱스

    // event
    public event Action<MapEntry> OnMapChanged;
    public void InvokeOnMapChanged() => OnMapChanged?.Invoke(CurrentMap);

    public MapEntry GetMapEntryOf(int mapIndex) => _mapEntries[mapIndex-1];
    public MapEntry GetMapEntryOf(string mapCode) => _mapEntries.FirstOrDefault(entry => entry.MapCode.Equals(mapCode));

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

        CurrentMapIndex = Mathf.Clamp(CurrentMapIndex, 1, MaximumUnlockedMapIndex);
        CurrentStageIndex = Mathf.Clamp(CurrentMapIndex, 1, MaximumUnlockedStageIndex);

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
            MaximumUnlockedMapIndex += 1;
            MaximumUnlockedStageIndex = 1;
        }

        OnMapChanged?.Invoke(CurrentMap);
    }

    public void Debug_SetCurrentMap(int mapIndex, int stageIndex)
    {
        if (mapIndex < 1 || mapIndex > MapCount || stageIndex < 1 || stageIndex > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(mapIndex));
        }

        MaximumUnlockedMapIndex = mapIndex;
        MaximumUnlockedStageIndex = stageIndex;

        //CurrentMapIndex = mapIndex;
        //CurrentStageIndex = stageIndex;

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

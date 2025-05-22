using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StageTimerSprites", menuName = "Scriptable Objects/Farm/StageTimerSprites")]
public sealed class StageTimerSprites : ScriptableObject
{
    [Serializable]
    public class StageOnOff
    {
        [SerializeField] public Sprite on;
        [SerializeField] public Sprite off;
    }
    
    [Serializable]
    public class MapOnOff
    {
        [SerializeField] public Sprite on;
        [SerializeField] public Sprite off;
    }

    [SerializeField] private StageOnOff[] stageOnOffs;
    [SerializeField] private MapOnOff[] mapOnOffs;

    public Sprite GetStageSprite(int stage, bool on)
    {
        if (stage < 1 || stage > stageOnOffs.Length) // stage-1하므로, 크기가 10일 때 10번 stage를 가져오는 것은 허용됨.
        {
            Debug.Log($"{stage}, {on} 에 해당하는 Stage Sprite를 찾을 수 없습니다.");
            return null;
        }

        return on ? stageOnOffs[stage - 1].on : stageOnOffs[stage - 1].off;
    }
    
    public Sprite GetMapSprite(int map, bool on)
    {
        if (map < 1 || map > mapOnOffs.Length) // map-1하므로, 크기가 3일 때 3번 map을 가져오는 것은 허용됨.
        {
            Debug.Log($"{map}, {on} 에 해당하는 Map Sprite를 찾을 수 없습니다.");
            return null;
        }

        return on ? mapOnOffs[map - 1].on : mapOnOffs[map - 1].off;
    }
}

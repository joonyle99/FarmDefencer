using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public Monster WaveMonster;
    public JoonyleGameDevKit.RangeInt SpawnCountRange;
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/TowerDefence/StageData", order = 1)]
public class StageData : ScriptableObject
{
    // Map -> Stage -> Wave -> Monster
    public List<WaveData> Waves = new List<WaveData>();
}
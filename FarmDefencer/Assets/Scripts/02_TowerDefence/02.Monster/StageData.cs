using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveMonsterData
{
    public Monster WaveMonster;
    public JoonyleGameDevKit.RangeInt SpawnCountRange;
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/TowerDefence/StageData", order = 0)]
public class StageData : ScriptableObject
{
    public List<WaveMonsterData> Waves = new List<WaveMonsterData>();
}
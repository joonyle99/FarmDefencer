using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/TowerDefence/MapData", order = 0)]
public class MapData : ScriptableObject
{
    // Map -> Stage -> Wave -> Monster
    public List<StageData> StateDataList = new List<StageData>();
}
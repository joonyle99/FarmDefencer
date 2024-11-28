using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// <see cref="ManagerClassGuideline"/>
/// </remarks>
public class TowerDefenceManager : JoonyleGameDevKit.Singleton<TowerDefenceManager>
{
    private List<string> _survivedMonsters = new List<string>();
    public List<string> SurvivedMonsters => _survivedMonsters;

    public int SurvivedCount => _survivedMonsters.Count;

    public event System.Action<int> OnSurvivedCountChanged;

    // remain monster
    public void AddSurvivedMonster(string monsterName)
    {
        _survivedMonsters.Add(monsterName);
        OnSurvivedCountChanged?.Invoke(_survivedMonsters.Count);
    }
    public void RemoveSurvivedMonster(string monsterName)
    {
        _survivedMonsters.Remove(monsterName);
        OnSurvivedCountChanged?.Invoke(_survivedMonsters.Count);
    }
}

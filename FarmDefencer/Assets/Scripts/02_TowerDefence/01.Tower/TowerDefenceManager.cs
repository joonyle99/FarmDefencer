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
    public int SurvivedCount => _survivedMonsters.Count;
    public event System.Action<int> OnSurvivedCountChanged;

    private void Update()
    {
        // CHEAT: fast clock
        if (Input.GetMouseButton(0))
        {
            Time.timeScale = 3f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

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

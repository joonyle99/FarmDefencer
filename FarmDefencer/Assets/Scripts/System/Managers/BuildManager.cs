using UnityEngine;

public class BuildManager : JoonyleGameDevKit.Singleton<BuildManager>
{
    [SerializeField] private Tower[] _towerPrefabs;

    private int _selectedTowerIndex = 0;

    public Tower GetSelectedTower()
    {
        return _towerPrefabs[_selectedTowerIndex];
    }
}

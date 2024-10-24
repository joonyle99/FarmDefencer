using UnityEngine;

/// <summary>
/// 타워 디펜스에서 타워의 건설을 관리합니다.
/// </summary>
/// <remarks>
/// Manager vs Supervisor에 대한 설명은 <see cref="ManagerVsSupervisor"/>를 참조하세요
/// </remarks>
public class BuildSupervisor : JoonyleGameDevKit.Singleton<BuildSupervisor>
{
    [SerializeField] private Tower[] _towerPrefabs;

    private int _selectedTowerIndex = 0;

    public Tower GetSelectedTower()
    {
        return _towerPrefabs[_selectedTowerIndex];
    }
}

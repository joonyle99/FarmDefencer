using UnityEngine;

/// <summary>
/// Ÿ�� ���潺���� Ÿ���� �Ǽ��� �����մϴ�.
/// </summary>
/// <remarks>
/// Supervisor �� ���� ������ <see cref="ManagerClassGuideline"/>�� �����ϼ���
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

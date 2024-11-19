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

    public Tower InstantiateTower(Vector3 targetPos, Quaternion targetRot)
    {
        var towerToBuild = GetSelectedTower();

        // check select
        if (towerToBuild == null)
        {
            Debug.LogWarning("There is no tower to build, you should select tower");
            return null;
        }

        var canBuild = towerToBuild.IsValidBuild(ResourceManager.Instance.GetGold());

        // check build
        if (canBuild == false)
        {
            Debug.LogWarning("You don't have enough gold to build this tower");
            return null;
        }

        var towerToReturn = Instantiate(towerToBuild, targetPos, targetRot);

        // check instantiate
        if (towerToReturn == null)
        {
            Debug.LogWarning("There is no tower to return, you should verify that the GameObject has been created");
            return null;
        }

        return towerToReturn;
    }
    public Tower GetSelectedTower()
    {
        return _towerPrefabs[_selectedTowerIndex];
    }
}

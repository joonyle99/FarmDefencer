using UnityEngine;

/// <summary>
/// 타워 건설을 관리하는 시스템
/// </summary>
public class TowerBuildSystem : MonoBehaviour
{
    [Header("━━━━━━━━ Tower Build System ━━━━━━━━")]
    [Space]

    [SerializeField] private Tower[] _towerPrefabs;
    [SerializeField] private int _selectedTowerIndex = 0;

    public Tower CheckTower()
    {
        var towerToBuild = _towerPrefabs[_selectedTowerIndex];

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
            Debug.Log("You don't have enough gold to build this tower");
            return null;
        }

        return towerToBuild;
    }
    public Tower InstantiateTower(Tower towerToBuild, Vector3 targetPos, Quaternion targetRot)
    {
        ResourceManager.Instance.SpendGold(towerToBuild.CurrentCost);

        var towerToInstance = Instantiate(towerToBuild, targetPos, targetRot);

        // check instantiate
        if (towerToInstance == null)
        {
            Debug.LogWarning("There is no tower to return, you should verify that the GameObject has been created");
            return null;
        }

        return towerToInstance;
    }
}
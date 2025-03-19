using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타워 건설을 관리하는 시스템
/// </summary>
public class BuildSystem : MonoBehaviour
{
    [Header("━━━━━━━━ Build System ━━━━━━━━")]
    [Space]

    [SerializeField] private int _selectedIndex = 0;
    [SerializeField] private Tower[] _towerPrefabs;
    public Tower SelectedTower => _towerPrefabs[_selectedIndex];

    // dynamic variable
    private Tower _ghostTower;
    private GridCell _hoveringGridCell;

    public void Pick(PointerEventData eventData)
    {
        if (BuildState() && EnoughGold(SelectedTower))
        {
            Vector3 worldPos = ConvertToWorldPos(eventData.position);
            CreateGhostTower(worldPos);
        }
    }
    public void Move(PointerEventData eventData)
    {
        if (_ghostTower == null)
        {
            return;
        }

        Vector3 worldPos = ConvertToWorldPos(eventData.position);
        GridCell gridCell = FindGridCell(worldPos);

        if (OutOfTileMap(worldPos) || NullGridCell(gridCell))
        {
            Debug.Log("OutOfTileMap || NullGridCell");
            MoveGhostTower(worldPos);

            _ghostTower.SpineController.SetColor(new Color(1, 0, 0, 0.7f));
            _hoveringGridCell = null;
        }
        else
        {
            if (SameGridCell(gridCell))
            {
                Debug.Log("Same GridCell");
                return;
            }
            else if (!EmptyGridCell(gridCell) || !UsableGridCell(gridCell))
            {
                Debug.Log("EmptyGridCell || UsableGridCell");
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.SetColor(new Color(1, 0, 0, 0.7f));
                _hoveringGridCell = gridCell;
            }
            else
            {
                Debug.Log("Valid GridCell");
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.SetColor(new Color(1, 1, 1, 0.7f));
                _hoveringGridCell = gridCell;
            }
        }
    }
    public void Place(PointerEventData eventData)
    {
        if (_ghostTower == null)
        {
            return;
        }

        if (_hoveringGridCell == null)
        {
            CancelBuild();
            return;
        }

        if (EnoughGold(_ghostTower) == false)
        {
            CancelBuild();
            return;
        }

        if (EmptyGridCell(_hoveringGridCell) == false || UsableGridCell(_hoveringGridCell) == false)
        {
            CancelBuild();
            return;
        }

        DefenceContext.Current.GridMap.LastGridCell = _hoveringGridCell;

        if (CheckBuild() == false)
        {
            CancelBuild();
            return;
        }

        PlaceGhostTower();

        _hoveringGridCell = null;
        _ghostTower = null;
    }

    //
    private GridCell FindGridCell(Vector3 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return null;
        return hit.collider.GetComponent<GridCell>();
    }

    //
    private Vector3 ConvertToWorldPos(Vector2 screenPos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPos);
        pos.z = 0;
        return pos;
    }

    //
    private void CreateGhostTower(Vector3 worldPos)
    {
        _ghostTower = Instantiate(SelectedTower, worldPos, Quaternion.identity);
        _ghostTower.SpineController.SetColor(new Color(1, 1, 1, 0.7f));
    }
    private void MoveGhostTower(Vector3 worldPos)
    {
        _ghostTower.transform.position = worldPos;
    }
    private void PlaceGhostTower()
    {
        _hoveringGridCell.Occupy(_ghostTower);
        _hoveringGridCell.UnUsable();

        _ghostTower.enabled = true;
        _ghostTower.SpineController.ResetColor();

        ResourceManager.Instance.SpendGold(_ghostTower.CurrentCost);
    }

    private void CancelBuild()
    {
        Debug.Log("CancelBuild");

        if (_ghostTower != null)
        {
            Debug.Log("1");

            Destroy(_ghostTower.gameObject);
            _ghostTower = null;
        }

        _hoveringGridCell = null;
    }

    //
    private bool BuildState()
    {
        return GameStateManager.Instance.CurrentState is GameState.Build
            || GameStateManager.Instance.CurrentState is GameState.Wave;
    }
    private bool CheckBuild()
    {
        return _hoveringGridCell.CheckBuild();
    }
    private bool EnoughGold(Tower tower)
    {
        return tower.HasEnoughGold();
    }

    //
    private bool OutOfTileMap(Vector3 worldPos)
    {
        return DefenceContext.Current.GridMap.IsOutOfTileMap(worldPos);
    }

    //
    private bool NullGridCell(GridCell newGridCell)
    {
        return newGridCell == null;
    }
    private bool SameGridCell(GridCell newGridCell)
    {
        return newGridCell == _hoveringGridCell;
    }
    private bool EmptyGridCell(GridCell newGridCell)
    {
        return newGridCell.IsEmpty;
    }
    private bool UsableGridCell(GridCell newGridCell)
    {
        return newGridCell.IsUsable;
    }
}
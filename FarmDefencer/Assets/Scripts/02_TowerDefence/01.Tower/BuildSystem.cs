using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타워 건설을 관리하는 시스템
/// </summary>
public class BuildSystem : MonoBehaviour
{
    [Header("━━━━━━━━ Build System ━━━━━━━━")]
    [Space]

    // build tower
    public int selectedIndex = 0;
    [SerializeField] private Tower[] _availableTowers;
    public Tower[] AvailableTowers => _availableTowers;
    public Tower SelectedTower => _availableTowers[selectedIndex];

    [Space]

    // progress bar
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private float _buildDuration = 20f;
    private float _buildTimer = 0f;

    // dynamic variable
    private Tower _ghostTower;
    private GridCell _hoveringGridCell;

    // Color 같은 구조체엔 const 대신 항상 static을 써야 함
    public static readonly Color RED_GHOST_COLOR = new Color(1f, 0f, 0f, 0.7f);
    public static readonly Color NORMAL_GHOST_COLOR = new Color(1f, 1f, 1f, 0.7f);
    public static readonly Color RED_RANGE_COLOR = new Color(1f, 0f, 0f, 0.8f);
    public static readonly Color BLUE_RANGE_COLOR = new Color(0f, 0f, 1f, 0.8f);

    private void Start()
    {
        _progressBar.Initialize();
        _progressBar.SetDangerousThreshold(0.5f);
    }
    private void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Build)
        {
            _buildTimer += Time.deltaTime;
            if (_buildTimer < _buildDuration)
            {
                var remainBuildTime = _buildDuration - _buildTimer;
                _progressBar.UpdateProgressBar(remainBuildTime, _buildDuration);
            }
            else
            {
                _buildTimer = 0f;
                _progressBar.UpdateProgressBar(0f, _buildDuration);

                GameStateManager.Instance.ChangeState(GameState.Wave);
                DefenceContext.Current.WaveSystem.StartWaveProcess();
            }
        }
    }

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
            MoveGhostTower(worldPos);

            _ghostTower.SpineController.SetColor(RED_GHOST_COLOR);
            _ghostTower.Detector.EraseRange();
            _hoveringGridCell = null;
        }
        else
        {
            if (SameGridCell(gridCell))
            {
                return;
            }
            else if (!EmptyGridCell(gridCell) || !UsableGridCell(gridCell))
            {
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.SetColor(RED_GHOST_COLOR);
                _ghostTower.Detector.PaintRange(RED_RANGE_COLOR);
                _hoveringGridCell = gridCell;
            }
            else
            {
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.ResetColor();
                _ghostTower.Detector.PaintRange(BLUE_RANGE_COLOR);
                _hoveringGridCell = gridCell;
            }
        }
    }
    public void Place(PointerEventData eventData)
    {
        _ghostTower.SpineController.ResetColor();
        _ghostTower.Detector.EraseRange();

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

        if (CheckBuild(_hoveringGridCell) == false)
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
    private bool CheckBuild(GridCell gridCell)
    {
        // gridCell을 기준으로 경로를 탐색하여 타워를 설치할 수 있는지 확인한다
        return gridCell.CheckBuild();
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
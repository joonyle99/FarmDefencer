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

    // build panel
    public PanelToggler PanelToggler; // // TODO: 구조 변경하기..

    // dynamic variable
    private Tower _ghostTower;
    private GridCell _hoveringGridCell;

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
                var id = MapManager.Instance.CurrentMap.MapId;
                if (id == 1)
                {
                    SoundManager.Instance.PlayBgm("BGM_D_forest_song", 0.7f);
                }
                else if (id == 2)
                {
                    SoundManager.Instance.PlayBgm("BGM_D_beach_song", 0.7f);
                }
                DefenceContext.Current.WaveSystem.StartWaveProcess();
            }
        }
    }

    // 타워를 집고, 옮기고, 놓는 기능을 처리하는 메소드들
    public void Pick(PointerEventData eventData)
    {
        // 빌드가 가능한 게임 상태인지, 돈이 충분한지 확인
        if (BuildableState() && EnoughGold(SelectedTower))
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

        // 열려 있는 타워 설치 패널을 닫기
        if (PanelToggler.IsExpanded)
        {
            PanelToggler.TogglePanel();
        }

        Vector3 worldPos = ConvertToWorldPos(eventData.position);
        GridCell gridCell = FindGridCell(worldPos);

        // 타일맵 밖이거나 해당 셀이 유효하지 않은 경우
        if (OutOfTileMap(worldPos) || NullGridCell(gridCell))
        {
            MoveGhostTower(worldPos);

            _ghostTower.SpineController.SetColor(ConstantConfig.RED_GHOST);
            _ghostTower.Detector.EraseRange();
            _hoveringGridCell = null;
        }
        else
        {
            // 같은 셀인 경우
            if (SameGridCell(gridCell))
            {
                return;
            }
            // 비어있지 않거나 사용 불가능(시작점 혹은 도착점 등)인 경우
            else if (!EmptyGridCell(gridCell) || !UsableGridCell(gridCell) || !CheckPath(gridCell))
            {
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.SetColor(ConstantConfig.RED_GHOST);
                _ghostTower.Detector.PaintRange(ConstantConfig.RED_RANGE);
                _hoveringGridCell = gridCell;
            }
            // 그 이외의 모든 경우는 빈 셀이고 사용 가능한 셀인 경우
            else
            {
                MoveGhostTower(gridCell.worldPosition);

                _ghostTower.SpineController.ResetColor();
                _ghostTower.Detector.PaintRange(ConstantConfig.BLUE_RANGE);
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

        _ghostTower.SpineController.ResetColor();
        _ghostTower.Detector.EraseRange();

        // 닫혀있는 타워 설치 패널을 열기
        if (!PanelToggler.IsExpanded)
        {
            PanelToggler.TogglePanel();
        }

        // 호버링 중인 셀이 없는 경우
        if (_hoveringGridCell == null)
        {
            CancelBuild();
            return;
        }

        // 돈이 충분하지 않은 경우
        if (!EnoughGold(_ghostTower))
        {
            CancelBuild();
            return;
        }

        // 셀이 비어있지 않거나 사용 불가능한 경우
        if (!EmptyGridCell(_hoveringGridCell) || !UsableGridCell(_hoveringGridCell))
        {
            CancelBuild();
            return;
        }

        DefenceContext.Current.GridMap.LastGridCell = _hoveringGridCell;

        // 
        if (!CheckPath(_hoveringGridCell))
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
        //Debug.Log("CancelBuild");

        if (_ghostTower != null)
        {
            Destroy(_ghostTower.gameObject);
            _ghostTower = null;
        }

        _hoveringGridCell = null;
    }

    //
    private bool BuildableState()
    {
        return GameStateManager.Instance.CurrentState is GameState.Build
            || GameStateManager.Instance.CurrentState is GameState.Wave
            || GameStateManager.Instance.CurrentState is GameState.WaveAfter;
    }
    private bool CheckPath(GridCell gridCell)
    {
        // gridCell을 기준으로 경로를 탐색하여 타워를 설치할 수 있는지 확인한다
        return gridCell.CheckPath();
    }
    private bool EnoughGold(Tower tower)
    {
        return tower.EnoughGold();
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
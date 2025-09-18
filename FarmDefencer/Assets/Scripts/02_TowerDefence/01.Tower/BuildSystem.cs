using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

/// <summary>
/// 타워 건설을 관리하는 시스템
/// </summary>
public class BuildSystem : MonoBehaviour, IVolumeControl
{
    private enum ColorState
    {
        None,
        Red,
        Blue,
    }

    [Header("━━━━━━━━ Build System ━━━━━━━━")]
    [Space]

    // build tower
    public int selectedIndex = 0;
    [SerializeField] private Tower[] _availableTowers;
    public Tower[] AvailableTowers => _availableTowers;
    public Tower SelectedTower => _availableTowers[selectedIndex];

    [Space]

    // build panel
    [SerializeField] private PanelToggler _panelToggler; // TODO: 구조 변경하기..

    [Space]

    // progress bar
    [SerializeField] private ProgressBar _progressBar;

    [Space]

    // build duration
    [SerializeField] private float _buildDuration = 20f;
    private float _buildTimer = 0f;

    // dynamic variable
    private Tower _ghostTower;
    private GridCell _occupiedCellByGhost;
    public GridCell OccupiedCellByGhost => _occupiedCellByGhost;

    private ColorState _prevSpineColorState;
    private ColorState _prevDetectorColorState;

    private ColorState _spineColorState;
    private ColorState _detectorColorState;

    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float buildSuccessVolume = 0.5f;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float buildFailureVolume = 0.5f;

    private void Awake()
    {
        GameStateManager.Instance?.AddCallback(GameState.Build, (Action)InitBuildTimer);
        GameStateManager.Instance?.AddCallback(GameState.Build, (Action)InitProgressBar);
    }
    private void Start()
    {
        _prevSpineColorState = ColorState.None;
        _prevDetectorColorState = ColorState.None;

        _spineColorState = ColorState.None;
        _detectorColorState = ColorState.None;
    }
    private void Update()
    {
        if (GameStateManager.Instance.IsBuildState)
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

                GameStateManager.Instance.ChangeState(GameState.WaveInProgress);
            }
        }

        if (GameStateManager.Instance.IsDefenceState)
        {
            if (_ghostTower != null)
            {
                if (true)
                {
                    if (_spineColorState == ColorState.None)
                    {
                        _ghostTower.SpineController.ResetColor();
                    }
                    else if (_spineColorState == ColorState.Blue)
                    {
                        _ghostTower.SpineController.SetColor(ConstantConfig.BLUE_GHOST);
                    }
                    else if (_spineColorState == ColorState.Red)
                    {
                        _ghostTower.SpineController.SetColor(ConstantConfig.RED_GHOST);
                    }
                }

                if (true)
                {
                    if (_detectorColorState == ColorState.None)
                    {
                        _ghostTower.Detector.EraseRange();
                    }
                    else if (_detectorColorState == ColorState.Blue)
                    {
                        _ghostTower.Detector.PaintRange(ConstantConfig.BLUE_RANGE);
                    }
                    else if (_detectorColorState == ColorState.Red)
                    {
                        _ghostTower.Detector.PaintRange(ConstantConfig.RED_RANGE);
                    }
                }

                if (_occupiedCellByGhost != null)
                {
                    var monsters = _occupiedCellByGhost.monstersInCell;
                    if (monsters.Count > 0)
                    {
                        _ghostTower.SpineController.SetColor(ConstantConfig.RED_GHOST);
                        _ghostTower.Detector.PaintRange(ConstantConfig.RED_RANGE);
                    }
                }
            }
        }
    }
    private void OnDestroy()
    {
        GameStateManager.Instance?.RemoveCallback(GameState.Build, (Action)InitBuildTimer);
        GameStateManager.Instance?.RemoveCallback(GameState.Build, (Action)InitProgressBar);
    }

    private void InitBuildTimer()
    {
        _buildTimer = 0f;
    }
    private void InitProgressBar()
    {
        _progressBar.Initialize();
        _progressBar.SetDangerousThreshold(0.5f);
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
        if (_panelToggler.IsExpanded)
        {
            _panelToggler.TogglePanel();
        }

        Vector3 worldPos = ConvertToWorldPos(eventData.position);
        GridCell gridCell = FindGridCell(worldPos);

        // 타일맵 밖이거나 해당 셀이 유효하지 않은 경우
        if (OutOfTileMap(worldPos) || NullGridCell(gridCell))
        {
            MoveGhostTower(worldPos);

            _prevSpineColorState = _spineColorState;
            _prevDetectorColorState = _detectorColorState;

            _spineColorState = ColorState.Red;
            _detectorColorState = ColorState.None;

            _occupiedCellByGhost = null;
        }
        else
        {
            // 같은 셀인 경우
            if (SameGridCell(gridCell))
            {
                return;
            }
            // 비어있지 않거나 사용 불가능(시작점 혹은 도착점 등)인 경우
            else if (!EmptyGridCell(gridCell) || !UsableGridCell(gridCell) || !CheckPath(gridCell, true))
            {
                MoveGhostTower(gridCell.worldPosition);

                _prevSpineColorState = _spineColorState;
                _prevDetectorColorState = _detectorColorState;

                _spineColorState = ColorState.Red;
                _detectorColorState = ColorState.Red;

                _occupiedCellByGhost = gridCell;
            }
            // 그 이외의 모든 경우는 빈 셀이고 사용 가능한 셀인 경우
            else
            {
                MoveGhostTower(gridCell.worldPosition);

                _prevSpineColorState = _spineColorState;
                _prevDetectorColorState = _detectorColorState;

                _spineColorState = ColorState.None;
                _detectorColorState = ColorState.Blue;

                _occupiedCellByGhost = gridCell;
            }
        }
    }
    public void Place(PointerEventData eventData)
    {
        if (_ghostTower == null)
        {
            return;
        }

        _prevSpineColorState = _spineColorState;
        _prevDetectorColorState = _detectorColorState;

        _spineColorState = ColorState.None;
        _detectorColorState = ColorState.None;

        _ghostTower.SpineController.ResetColor();
        _ghostTower.Detector.EraseRange();

        // 닫혀있는 타워 설치 패널을 열기
        if (!_panelToggler.IsExpanded)
        {
            _panelToggler.TogglePanel();
        }

        // 호버링 중인 셀이 없는 경우
        if (_occupiedCellByGhost == null)
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
        if (!EmptyGridCell(_occupiedCellByGhost) || !UsableGridCell(_occupiedCellByGhost))
        {
            CancelBuild();
            return;
        }

        DefenceContext.Current.GridMap.LastPlacedGridCell = _occupiedCellByGhost;

        //
        if (!CheckPath(_occupiedCellByGhost))
        {
            CancelBuild();
            return;
        }

        PlaceGhostTower();

        DefenceContext.Current.GridMap.LastPlacedGridCell = null;

        _occupiedCellByGhost = null;
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

    // TODO: 타워가 동작하지 않도록 해야한다.. (활성화 / 비활성화)
    private void CreateGhostTower(Vector3 worldPos)
    {
        _ghostTower = Instantiate(SelectedTower, worldPos, Quaternion.identity);
        _ghostTower.Deactivate();
    }
    private void MoveGhostTower(Vector3 worldPos)
    {
        _ghostTower.transform.position = worldPos;
    }
    private void PlaceGhostTower()
    {
        _occupiedCellByGhost.Occupy(_ghostTower);
        _occupiedCellByGhost.UnUsable();

        _ghostTower.Activate();
        _ghostTower.SpineController.ResetColor();

        SoundManager.Instance.PlaySfx($"SFX_D_tower_build", buildSuccessVolume);

        ResourceManager.Instance.SpendGold(_ghostTower.CurrentCost);
    }

    private void CancelBuild()
    {
        //Debug.Log("CancelBuild");

        if (_ghostTower != null)
        {
            SoundManager.Instance.PlaySfx($"SFX_D_tower_fail", buildSuccessVolume);

            Destroy(_ghostTower.gameObject);
            _ghostTower = null;
        }

        _occupiedCellByGhost = null;
    }

    //
    private bool BuildableState()
    {
        return GameStateManager.Instance.CurrentState is GameState.Build
            || GameStateManager.Instance.CurrentState is GameState.WaveInProgress
            || GameStateManager.Instance.CurrentState is GameState.WaveCompleted;
    }
    private bool CheckPath(GridCell gridCell, bool isDirty = false)
    {
        // gridCell을 기준으로 경로를 탐색하여 타워를 설치할 수 있는지 확인한다
        return gridCell.CheckPath(isDirty);
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
        return newGridCell == _occupiedCellByGhost;
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
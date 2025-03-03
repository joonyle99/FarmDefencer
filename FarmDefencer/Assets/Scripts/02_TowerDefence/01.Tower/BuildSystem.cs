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
    private GridCell _hoverCell;
    private bool _reallyCanBuild = false;

    private void Update()
    {
        if (_ghostTower != null)
        {
            // _reallyCanBuild를 설정하는 로직 작성
            // Update에서 실시간으로 경로를 검사하여 타워 설치 가능 여부 확인
            // TODO: 그런데 매 프레임 마다 검사하는 것은 좀 그렇고,, 몬스터가 셀을 움직여서 새로운 경로를 만들 때만 검사하면 될 것 같다
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 현재 타워 건설 상태가 아니라면 이벤트를 처리하지 않는다
        if (GameStateManager.Instance.CurrentState is not GameState.Build
            && GameStateManager.Instance.CurrentState is not GameState.Wave)
        {
            return;
        }

        // 돈이 충분한지 확인
        var isEnoughGold = CheckEnoughGold(SelectedTower);
        if (isEnoughGold == false)
        {
            return;
        }

        // 타워 생성
        Vector3 worldPos = GetWorldPos(eventData.position);
        _ghostTower = Instantiate(SelectedTower, worldPos, Quaternion.identity);
        Color ghostColor = new Color(1, 1, 1, 0.7f);
        _ghostTower.SpineController.SetColor(ghostColor);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_ghostTower == null) return;
        if (_hoverCell == null) // GridCell에 한 번도 들어가지 않은 경우
        {
            Destroy(_ghostTower);
            return;
        }

        // 실시간으로 경로를 검사하여 타워 설치 가능 여부 확인
        var canBuild = _hoverCell.CheckBuild();
        if (canBuild == false)
        {
            _ghostTower.Detector.EraseRange();
            Destroy(_ghostTower);
        }
        else
        {
            _hoverCell.Occupy(_ghostTower);
            _hoverCell.UnUsable();
            _ghostTower.enabled = true;
            _ghostTower.SpineController.ResetColor();
            ResourceManager.Instance.SpendGold(_ghostTower.CurrentCost);
        }

        _hoverCell.OffHover();
        _hoverCell = null;
        _ghostTower = null;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_ghostTower == null) return;

        // 마우스 위치를 월드 좌표로 변환
        Vector3 worldPos = GetWorldPos(eventData.position);

        // 타일맵 바깥에서의 타워 위치 업데이트
        if (DefenceContext.Current.GridMap.IsOutOfTileMap(worldPos))
        {
            // 이전에 Hover된 GridCell이 있을 경우
            if (_hoverCell != null)
            {
                _hoverCell.OffHover();
                _ghostTower.Detector.EraseRange();
                _hoverCell = null;
            }

            _ghostTower.transform.position = worldPos;
        }

        // 현재 마우스 위치에 해당하는 GridCell 찾기
        GridCell newHoverCell = FindGridCell(worldPos);

        // 타워를 설치할 수 있는 GridCell인지 확인
        if (newHoverCell == null) return;
        if (newHoverCell == _hoverCell) return;
        if (newHoverCell.IsEmptyUsableCell == false) return;

        // GridMap에 마지막 Hover된 GridCell 업데이트
        DefenceContext.Current.GridMap.LastHoverCell = newHoverCell;

        // GridCell에 첫 진입(Hover)인 경우 null일 수 있기 때문
        if (_hoverCell != null)
        {
            _hoverCell.OffHover(); // '이전'의 GridCell을 Hover Off 해주기 위함
            _ghostTower.Detector.EraseRange(); // '이전' 위치의 타워 범위 삭제
        }

        // Hover Cell 업데이트
        _hoverCell = newHoverCell;
        _hoverCell.OnHover();

        // 타일맵 안쪽에서의 타워 위치 업데이트
        _ghostTower.transform.position = newHoverCell.worldPosition;
        _ghostTower.Detector.PaintRange();
    }

    private Vector3 GetWorldPos(Vector2 screenPos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPos);
        pos.z = 0;
        return pos;
    }
    private GridCell FindGridCell(Vector3 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return null;
        return hit.collider.GetComponent<GridCell>();
    }
    private bool CheckEnoughGold(Tower towerToBuild)
    {
        var hasEnoughGold = towerToBuild.HasEnoughGold();

        if (hasEnoughGold == false)
        {
            Debug.Log("You don't have enough gold to build this tower");
            return false;
        }

        return true;
    }
}
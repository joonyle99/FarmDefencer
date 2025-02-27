using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타워를 건설할 수 있는 부지를 나타내는 컴포넌트입니다.
/// 마우스입력 및 터치입력을 통해 타워 건설 기능을 제공합니다
/// </summary>
/// <remarks>
/// - 클릭 시 BuildSupervisor에서 선택된 타워를 해당 위치에 건설합니다.
/// </remarks>
public class GridCell : MonoBehaviour
{
    [Header("──────── GridCell ────────")]
    [Space]

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hoverColor;
    private Color _initColor;
    private Color _startColor;

    private Tower _occupiedTower;

    [Space]

    public TextMeshPro distanceCostText;
    public TextMeshPro isUsableText;

    [Space]

    public GridCell prevGridCell;
    public Vector2Int cellPosition;
    public Vector3 worldPosition;

    [Space]

    public int distanceCost;
    public bool isUsable;

    private int _changedColorReferenceCount = 0;

    private void Start()
    {
        _initColor = _spriteRenderer.color;
        _startColor = _spriteRenderer.color;

        distanceCostText.text = "";
        isUsableText.text = "";
    }
    private void Update()
    {
        //distanceCostText.text = distanceCost.ToString($"D{2}");
        //isUsableText.text = isUsable.ToString();
        //isUsableText.color = (isUsable == true) ? Color.blue : Color.red;
    }

    private void OnMouseEnter()
    {
        // 현재 포인터가 GridCell 위에 있지 않다면(UI 요소 위에 있다면) 이벤트를 처리하지 않는다
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        // 타워가 설치되어 있거나 사용할 수 없는 상태라면 이벤트를 처리하지 않는다
        if (_occupiedTower != null || isUsable == false)
        {
            return;
        }

        OnHover();
    }
    private void OnMouseExit()
    {
        // 현재 포인터가 GridCell 위에 있지 않다면(UI 요소 위에 있다면) 이벤트를 처리하지 않는다
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        OffHover();
    }

    private void OnMouseDown()
    {
        // 현재 타워 건설 상태가 아니라면 이벤트를 처리하지 않는다
        if (GameStateManager.Instance.CurrentState is not GameState.Build
            && GameStateManager.Instance.CurrentState is not GameState.Wave)
        {
            return;
        }

        // 현재 포인터가 GridCell 위 (UI 요소 위) 에 있지 않다면 이벤트를 처리하지 않는다
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        // GridMap에 현재 클릭된 GridCell을 저장
        DefenceContext.Current.GridMap.ClickedCell = this;

        // 타워가 설치되어 있다면 패널을 보여준다
        if (_occupiedTower != null)
        {
            _occupiedTower.ShowPanel();
        }
        // 타워가 설치되어 있지 않다면 설치를 시도한다
        else if (_occupiedTower == null && isUsable == true)
        {
            TowerBuildSystem towerBuildSystem = DefenceContext.Current.TowerBuildSystem;

            // 1. 설치할 타워의 유효성 및 건설 가능 여부 확인
            Tower towerToBuild = towerBuildSystem.CheckTower();
            if (towerToBuild == null)
            {
                Debug.Log("towerToBuild is invalid");
                return;
            }

            // 2. 실시간 경로 검사
            if (GameStateManager.Instance.CurrentState is GameState.Wave)
            {
                if (CheckBuild() == false)
                {
                    return;
                }
            }

            // 3. 설치할 타워의 인스턴스 생성 가능 여부 확인
            Tower towerToInstance = towerBuildSystem.InstantiateTower(towerToBuild, this.worldPosition, Quaternion.identity);
            if (towerToInstance == null)
            {
                Debug.Log("towerToInstance is invalid");
                return;
            }

            Occupy(towerToInstance);

            OffHover();
            UnUsable();
        }
        else
        {
            // 타워가 점유하고 있지는 않고, 사용할 수 없는 상태
            // e.g) start / end point
            Debug.Log($"here is start / end point");
        }
    }

    // check
    private bool CheckBuild()
    {
        var gridMap = DefenceContext.Current.GridMap;
        var fieldMonsters = DefenceContext.Current.WaveSystem.FieldMonsters;

        // 현재 클릭된 GridCell에 몬스터가 있는지 확인
        foreach (var fieldMonster in fieldMonsters)
        {
            bool isInCell = gridMap.IsTargetInCell(fieldMonster.transform.position, this);
            if (isInCell == true)
            {
                Debug.Log("a monster is in this grid cell");
                return false;
            }
        }

        isUsable = false; // 타워를 설치하려고 하는 곳을 경로 타일로 사용하지 않도록 하기 위함

        bool result = gridMap.FindPathAll();
        if (result == false)
        {
            isUsable = true;

            Debug.Log("failed to find path (origin path or each path)");
            return false;
        }

        isUsable = true;

        return true;
    }

    // sprite
    private void OnHover()
    {
        if (_spriteRenderer.color == _hoverColor)
        {
            return;
        }

        _spriteRenderer.color = _hoverColor;
    }
    private void OffHover()
    {
        if (_spriteRenderer.color == _startColor)
        {
            return;
        }

        _spriteRenderer.color = _startColor;
    }
    private void Appear()
    {
        var color = _spriteRenderer.color;
        color.a = _startColor.a;
        _spriteRenderer.color = color;
    }
    private void Disappear()
    {
        var color = _spriteRenderer.color;
        color.a = 0f;
        _spriteRenderer.color = color;
    }

    //
    public void Usable()
    {
        isUsable = true;
        Appear();
        // gameObject.SetActive(true);
    }
    public void UnUsable()
    {
        isUsable = false;
        Disappear();
        // gameObject.SetActive(false);
    }

    //
    public void Occupy(Tower tower)
    {
        _occupiedTower = tower;
        _occupiedTower.OccupyingGridCell(this);

        SoundManager.Instance.PlaySfx($"SFX_D_{_occupiedTower.TowerName}_build");
    }
    public void DeleteOccupiedTower()
    {
        _occupiedTower = null;
    }

    // debug
    public void DebugChangeColor(Color color)
    {
        _changedColorReferenceCount++;

        _spriteRenderer.color = color;
        _startColor = _spriteRenderer.color;
    }
    public void DebugResetColor()
    {
        _changedColorReferenceCount--;

        if (_changedColorReferenceCount == 0)
        {
            _spriteRenderer.color = _initColor;
            _startColor = _spriteRenderer.color;
        }
    }
}

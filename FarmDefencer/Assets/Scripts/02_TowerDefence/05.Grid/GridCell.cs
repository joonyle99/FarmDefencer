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

    public TextMeshPro textMeshPro;

    [Space]

    public GridCell prevGridCell;
    public Vector2Int cellPosition;

    [Space]

    public bool isUsable;
    public int distanceCost;

    private int _changedColorReferenceCount = 0;

    private void Start()
    {
        _initColor = _spriteRenderer.color;
        _startColor = _spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (_occupiedTower != null || isUsable == false)
        {
            return;
        }

        OnHover();
    }
    private void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        OffHover();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (_occupiedTower != null)
        {
            // 강화 UI는 어디에 띄워야 하는가..?

            // 타워가 점유되어 있는 상태, 클릭 시 강화 메뉴를 띄운다
            _occupiedTower.ShowPanel();
        }
        else if (_occupiedTower == null && isUsable == true)
        {
            // 타워가 점유되어 있지 않고, 사용할 수 있는 상태
            Occupy();
        }
        else
        {
            // 타워가 점유하고 있지는 않고, 사용할 수 없는 상태
            // e.g) start / end point
        }
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

    public void Occupy()
    {
        _occupiedTower = DefenceContext.Current.BuildSystem.InstantiateTower(transform.position, Quaternion.identity);
        _occupiedTower.OccupyingGridCell(this);

        if (_occupiedTower != null)
        {
            OffHover();
            UnUsable();
        }
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

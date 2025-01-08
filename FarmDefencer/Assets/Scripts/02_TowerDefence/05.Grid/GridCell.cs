using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ÿ���� �Ǽ��� �� �ִ� ������ ��Ÿ���� ������Ʈ�Դϴ�.
/// ���콺�Է� �� ��ġ�Է��� ���� Ÿ�� �Ǽ� ����� �����մϴ�
/// </summary>
/// <remarks>
/// - Ŭ�� �� BuildSupervisor���� ���õ� Ÿ���� �ش� ��ġ�� �Ǽ��մϴ�.
/// </remarks>
public class GridCell : MonoBehaviour
{
    [Header("���������������� GridCell ����������������")]
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
            // ��ȭ UI�� ��� ����� �ϴ°�..?

            // Ÿ���� �����Ǿ� �ִ� ����, Ŭ�� �� ��ȭ �޴��� ����
            _occupiedTower.ShowPanel();
        }
        else if (_occupiedTower == null && isUsable == true)
        {
            // Ÿ���� �����Ǿ� ���� �ʰ�, ����� �� �ִ� ����
            Occupy();
        }
        else
        {
            // Ÿ���� �����ϰ� ������ �ʰ�, ����� �� ���� ����
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

using TMPro;
using UnityEngine;

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
    private Color _startColor;

    private Tower _occupiedTower;

    [Space]

    public TextMeshPro textMeshPro;

    [Space]

    public GridCell prevGridCell;
    public Vector2Int cellPosition;

    [Space]

    public bool isMovable;
    public int distanceCost;

    private void Start()
    {
        _startColor = _spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        if (_occupiedTower != null)
        {
            return;
        }

        OnHover();
    }
    private void OnMouseExit()
    {
        OffHover();
    }
    private void OnMouseDown()
    {
        if (_occupiedTower != null)
        {
            Debug.LogWarning("Already tower has occupied, you should build other plot place");
            return;
        }

        Occupy();
    }

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

    public void Usable()
    {
        isMovable = true;
        // gameObject.SetActive(true);
    }
    public void UnUsable()
    {
        isMovable = false;
        // gameObject.SetActive(false);
    }

    public void Occupy()
    {
        _occupiedTower = BuildSupervisor.Instance.InstantiateTower(transform.position, Quaternion.identity);

        if (_occupiedTower != null)
        {
            OffHover();
            UnUsable();
        }
    }
}

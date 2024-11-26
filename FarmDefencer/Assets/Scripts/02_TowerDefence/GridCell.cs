using TMPro;
using UnityEngine;

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

    private void Start()
    {
        _startColor = _spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        if (isUsable == false)
        {
            return;
        }

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
        if (isUsable == false)
        {
            return;
        }

        if (_occupiedTower != null)
        {
            Debug.LogWarning("Already tower has occupied, you should build other plot place");
            return;
        }

        Occupy();
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
        _occupiedTower = BuildSupervisor.Instance.InstantiateTower(transform.position, Quaternion.identity);

        if (_occupiedTower != null)
        {
            OffHover();
            UnUsable();
        }
    }

    // debug
    public void DebugChangeColor(Color color)
    {
        if (_spriteRenderer.color == color)
        {
            return;
        }

        _spriteRenderer.color = color;
        _startColor = _spriteRenderer.color;
    }
}

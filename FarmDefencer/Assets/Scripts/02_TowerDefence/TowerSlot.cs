using UnityEngine;

/// <summary>
/// Ÿ���� �Ǽ��� �� �ִ� ������ ��Ÿ���� ������Ʈ�Դϴ�.
/// ���콺�Է� �� ��ġ�Է��� ���� Ÿ�� �Ǽ� ����� �����մϴ�
/// </summary>
/// <remarks>
/// - Ŭ�� �� BuildSupervisor���� ���õ� Ÿ���� �ش� ��ġ�� �Ǽ��մϴ�.
/// </remarks>
public class TowerSlot : MonoBehaviour
{
    [Header("���������������� Tower Slot ����������������")]
    [Space]

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hoverColor;
    private Color _startColor;

    private Tower _occupiedTower;

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

        _spriteRenderer.color = _hoverColor;
    }
    private void OnMouseExit()
    {
        _spriteRenderer.color = _startColor;
    }
    private void OnMouseDown()
    {
        if (_occupiedTower != null)
        {
            Debug.LogWarning("Already tower has occupied, you should build other plot place");
            return;
        }

        _occupiedTower = BuildSupervisor.Instance.InstantiateTower(transform.position, Quaternion.identity);
    }
}

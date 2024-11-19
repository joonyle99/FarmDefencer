using UnityEngine;

/// <summary>
/// 타워를 건설할 수 있는 부지를 나타내는 컴포넌트입니다.
/// 마우스입력 및 터치입력을 통해 타워 건설 기능을 제공합니다
/// </summary>
/// <remarks>
/// - 클릭 시 BuildSupervisor에서 선택된 타워를 해당 위치에 건설합니다.
/// </remarks>
public class TowerSlot : MonoBehaviour
{
    [Header("──────── Tower Slot ────────")]
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

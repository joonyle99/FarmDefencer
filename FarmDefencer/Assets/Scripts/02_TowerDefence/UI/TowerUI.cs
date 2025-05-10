using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("━━━━━━━━ Tower UI ━━━━━━━━")]
    [Space]

    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _cost;

    [SerializeField] private Color _hoverColor;
    private Color _normalColor = new Color(1f, 1f, 1f, 1f);

    [Space]

    [SerializeField] private int selectedIndex = 0;
    [SerializeField] private BuildSystem _buildSystem;

    private void Start()
    {
        var targetTowers = _buildSystem.AvailableTowers;
        var defaultLevel = targetTowers[selectedIndex].LevelData[0].ValueCost;
        _cost.text = defaultLevel.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // background, icon 색상을 어둡게 변경
        _background.color = _hoverColor;
        _icon.color = _hoverColor;

        _buildSystem.selectedIndex = selectedIndex;
        _buildSystem.Pick(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        _buildSystem.Move(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        // background, icon 색상을 어둡게 원래로 변경
        _background.color = _normalColor;
        _icon.color = _normalColor;

        _buildSystem.Place(eventData);
    }
}

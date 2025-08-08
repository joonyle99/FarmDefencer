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
    [SerializeField] private Color _hoverColor;
    private Color _normalColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private TextMeshProUGUI _cost;
    [SerializeField] private int selectedIndex = 0;

    private void Start()
    {
        var availableTowers = DefenceContext.Current.BuildSystem.AvailableTowers;
        var targetTower = availableTowers[selectedIndex];

        var deaultIcon = targetTower.DefaultLevelData.Icon;
        var defaultCost = targetTower.DefaultLevelData.ValueCost;

        _icon.sprite = deaultIcon;
        _cost.text = defaultCost.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // background, icon 색상을 어둡게 변경
        _background.color = _hoverColor;
        _icon.color = _hoverColor;

        DefenceContext.Current.BuildSystem.selectedIndex = selectedIndex;
        DefenceContext.Current.BuildSystem.Pick(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        DefenceContext.Current.BuildSystem.Move(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        // background, icon 색상을 어둡게 원래로 변경
        _background.color = _normalColor;
        _icon.color = _normalColor;

        DefenceContext.Current.BuildSystem.Place(eventData);
    }
}

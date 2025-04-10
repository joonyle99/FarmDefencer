using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PanelToggler : MonoBehaviour
{
    [Header("──────── Panel Toggler ────────")]
    [Space]

    [SerializeField] private RectTransform _panel;
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Sprite _expandedSprite;
    [SerializeField] private Sprite _collapsedSprite;

    [Space]

    [SerializeField] private float _expandedY;
    [SerializeField] private float _collapsedY;
    [SerializeField] private float _duration;

    [Space]

    [SerializeField] private bool _isExpanded;

    private void Start()
    {
        var targetY = _isExpanded ? _expandedY : _collapsedY;
        var panelPos = _panel.anchoredPosition;
        panelPos.y = targetY;
        _panel.anchoredPosition = panelPos;

        var targetSprite = _isExpanded ? _expandedSprite : _collapsedSprite;
        _buttonImage.sprite = targetSprite;
    }
    public void TogglePanel()
    {
        _isExpanded = !_isExpanded;

        var targetY = _isExpanded ? _expandedY : _collapsedY;
        //var ease = _isExpanded ? Ease.OutBack : Ease.InCubic;
        _panel.DOAnchorPosY(targetY, _duration).SetEase(Ease.OutBack);

        var targetSprite = _isExpanded ? _expandedSprite : _collapsedSprite;
        _buttonImage.sprite = targetSprite;
    }
}

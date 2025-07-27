using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PanelToggler : MonoBehaviour, IVolumeControl
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
    public bool IsExpanded => _isExpanded;

    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float expandVolume = 0.5f;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float collapseVolume = 0.5f;

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

        // 상태에 따라 사운드 재생
        if (_isExpanded)
        {
            SoundManager.Instance.PlaySfx("SFX_D_expand", expandVolume);
        }
        else
        {
            SoundManager.Instance.PlaySfx("SFX_D_collapse", collapseVolume);
        }

        var targetY = _isExpanded ? _expandedY : _collapsedY;
        //var ease = _isExpanded ? Ease.OutBack : Ease.InCubic;
        _panel.DOAnchorPosY(targetY, _duration).SetEase(Ease.OutBack);

        var targetSprite = _isExpanded ? _expandedSprite : _collapsedSprite;
        _buttonImage.sprite = targetSprite;
    }
}

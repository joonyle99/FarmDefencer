using UnityEngine;
using System;

/// <summary>
/// 0번 자식은 SpriteRenderer를 가진 LockSprite 오브젝트,
/// 1번 자식은 TMP_Text를 가진 RemainingTimeText 오브젝트
/// </summary>
public sealed class CropDisplay : MonoBehaviour
{
    public bool ManualMode { get; set; }
    private SpriteRenderer _remainingGauge;
    private SpriteRenderer _lockSprite;
    private GameObject _gaugeObject;
    private Func<float?> _getAutoModeRatio;

    public void UpdateGauge(float ratio)
    {
        var clamped = Mathf.Clamp01(ratio);
        _remainingGauge.transform.localScale = new Vector3(clamped, 1.0f, 1.0f);
    }
    
    public void Init(Func<float?> getAutoModeRatio) => _getAutoModeRatio = getAutoModeRatio;
    
    private void Awake()
    {
        _gaugeObject = transform.Find("Gauge").gameObject;
        _lockSprite = transform.Find("LockSprite").GetComponent<SpriteRenderer>();
        _remainingGauge = transform.Find("Gauge/Remaining").GetComponent<SpriteRenderer>();
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (ManualMode)
        {
            _gaugeObject.SetActive(true);
            _lockSprite.gameObject.SetActive(true);
        }
        else
        {
            _lockSprite.gameObject.SetActive(false);
            _gaugeObject.SetActive(_getAutoModeRatio is not null);
            if (_getAutoModeRatio is not null)
            {
                var gaugeRatio = _getAutoModeRatio();
                if (gaugeRatio is not null)
                {
                    UpdateGauge(gaugeRatio.Value);
                }
                else
                {
                    _gaugeObject.SetActive(false);
                }
            }
        }
    }
}

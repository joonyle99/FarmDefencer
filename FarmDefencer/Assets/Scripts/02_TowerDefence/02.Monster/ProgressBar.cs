using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("──────── Progress Bar ────────")]
    [Space]

    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _dangerSprite;

    private Image[] _images;

    private float _dangerousThreshold;
    private bool _isDangerous;
    private bool _isFinished;

    const float EPSILON = 0.01f;

    public Action OnStart;
    public Action OnFinished;
    public Action OnDangerous;

    private void Awake()
    {
        _images = GetComponentsInChildren<Image>();
    }
    public void Initialize()
    {
        _isDangerous = false;
        _isFinished = false;

        ChangeToNormal();

        OnStart?.Invoke();
    }

    public void ChangeToNormal()
    {
        if (_bar.sprite == _normalSprite) return;
        _bar.sprite = _normalSprite;
    }
    public void ChangeToDanger()
    {
        if (_bar.sprite == _dangerSprite) return;
        _bar.sprite = _dangerSprite;
    }

    /// <summary>
    /// 위험한 상태의 임계값을 설정
    /// </summary>
    public void SetDangerousThreshold(float threshold)
    {
        _dangerousThreshold = threshold;
    }

    /// <summary>
    /// currentValue == maxValue -> 진행도 0%
    /// </summary>
    public void UpdateProgressBar(float currentValue, float maxValue)
    {
        // fillAmount가 0인 경우 -> 진행도 100%
        // fillAmount가 0.6인 경우 -> 진행도 40%
        // fillAmount가 1인 경우 -> 진행도 0%
        var fillAmount = currentValue / maxValue;
        _bar.fillAmount = fillAmount;

        if (!_isDangerous && fillAmount <= _dangerousThreshold + EPSILON)
        {
            //Debug.Log($"fillAmount: {fillAmount}");
            //Debug.Log($"_dangerousThreshold: {_dangerousThreshold}");

            _isDangerous = true;
            OnDangerous?.Invoke();

            ChangeToDanger();
        }

        if (!_isFinished && fillAmount <= 0.0f + EPSILON)
        {
            _isFinished = true;
            OnFinished?.Invoke();
        }
    }

    public float GetBarWidth()
    {
        return _bar.GetComponent<RectTransform>().rect.width;
    }
    public float GetProgress()
    {
        // 프로그레스 진행도를 반환
        // 0: 끝 / 1: 시작
        // 1 -----> 0 으로 향한다
        return 1f - _bar.fillAmount;
    }

    public void SetAlpha(float a)
    {
        if (_images == null || _images.Length == 0)
        {
            return;
        }

        foreach (var image in _images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, a);
        }
    }
}

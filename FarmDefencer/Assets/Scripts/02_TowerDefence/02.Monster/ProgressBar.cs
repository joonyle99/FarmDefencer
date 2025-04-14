using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("──────── Progress Bar ────────")]
    [Space]

    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _dangerSprite;

    private float _dangerousAmount;
    private bool _isDangerous;
    private bool _isFinished;

    const float EPSILON = 0.01f;

    public Action OnDangerous;
    public Action OnFinished;

    private void OnEnable()
    {
        _isDangerous = false;
        _isFinished = false;
        ChangeToNormal();
    }

    public void UpdateProgressBar(float currentValue, float maxValue)
    {
        // fillAmount가 0인 경우 -> 진행도 100%
        // fillAmount가 0.6인 경우 -> 진행도 40%
        // fillAmount가 1인 경우 -> 진행도 0%
        var fillAmount = currentValue / maxValue;
        _bar.fillAmount = fillAmount;

        if (!_isDangerous && fillAmount <= _dangerousAmount + EPSILON)
        {
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

    public void SetDangerousAmount(float amount)
    {
        _dangerousAmount = amount;
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
}

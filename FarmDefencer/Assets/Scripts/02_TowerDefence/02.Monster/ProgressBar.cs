using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("──────── Progress Bar ────────")]
    [Space]

    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _dangerSprite;

    private void OnEnable()
    {
        ChangeToNormal();
    }

    public void UpdateProgressBar(float currentValue, float maxValue)
    {
        // fillAmount가 0인 경우 -> 진행도 100%
        // fillAmount가 1인 경우 -> 진행도 0%
        _bar.fillAmount = currentValue / maxValue;
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

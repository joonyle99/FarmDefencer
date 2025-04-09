using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("──────── Progress Bar ────────")]
    [Space]

    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _normalBar;
    [SerializeField] private Sprite _dangerBar;

    private void OnEnable()
    {
        ChangeToNormalBar();
    }

    public void UpdateProgressBar(float currentValue, float maxValue)
    {
        // 0 ~ 1
        _bar.fillAmount = currentValue / maxValue;
    }

    public void ChangeToNormalBar()
    {
        if (_bar.sprite == _normalBar) return;
        _bar.sprite = _normalBar;
    }
    public void ChangeToDangerBar()
    {
        if (_bar.sprite == _dangerBar) return;
        _bar.sprite = _dangerBar;
    }
}

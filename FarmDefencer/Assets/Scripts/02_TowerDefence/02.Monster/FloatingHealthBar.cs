using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _greenBar;
    [SerializeField] private Sprite _redBar;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        // 0 ~ 1
        _bar.fillAmount = currentValue / maxValue;
    }

    public void ChangeToGreenBar()
    {
        _bar.sprite = _greenBar;
    }
    public void ChangeToRedBar()
    {
        _bar.sprite = _redBar;
    }
}

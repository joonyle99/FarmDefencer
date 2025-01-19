using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Image _bar;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        // 0 ~ 1
        _bar.fillAmount = currentValue / maxValue;
    }
}

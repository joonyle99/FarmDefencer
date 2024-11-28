using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        // 0 ~ 1
        _slider.value = currentValue / maxValue;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("──────── Floating Health Bar ────────")]
    [Space]

    [SerializeField] private Image _bar;
    [SerializeField] private Sprite _greenBar;
    [SerializeField] private Sprite _redBar;

    private void OnEnable()
    {
        ChangeToGreenBar();
    }

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        // 0 ~ 1
        _bar.fillAmount = currentValue / maxValue;
    }

    public void ChangeToGreenBar()
    {
        if (_bar.sprite == _greenBar) return;
        //Debug.Log("ChangeToGreenBar");
        _bar.sprite = _greenBar;
    }
    public void ChangeToRedBar()
    {
        if (_bar.sprite == _redBar) return;
        //Debug.Log("ChangeToRedBar");
        _bar.sprite = _redBar;
    }
}

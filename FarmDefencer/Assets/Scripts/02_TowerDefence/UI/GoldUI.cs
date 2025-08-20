using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    private void OnEnable()
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        SetGoldText(ResourceManager.Instance.Coin);

        ResourceManager.Instance.OnCoinChanged -= SetGoldText;
        ResourceManager.Instance.OnCoinChanged += SetGoldText;
    }
    private void OnDisable()
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        ResourceManager.Instance.OnCoinChanged -= SetGoldText;
    }

    public void SetGoldText(int gold)
    {
        goldText.text = gold.ToString();
    }
}

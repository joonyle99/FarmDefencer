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

        SetGoldText(ResourceManager.Instance.Gold);

        ResourceManager.Instance.OnGoldChanged -= SetGoldText;
        ResourceManager.Instance.OnGoldChanged += SetGoldText;
    }
    private void OnDisable()
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        ResourceManager.Instance.OnGoldChanged -= SetGoldText;
    }

    public void SetGoldText(int gold)
    {
        goldText.text = gold.ToString();
    }
}

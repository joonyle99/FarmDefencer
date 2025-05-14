using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    // tower info - icon
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Space]

    // tower info - stats
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _powerText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _slowText;

    [Space]

    // tower info - action
    [SerializeField] private TextMeshProUGUI _sellCostText;
    [SerializeField] private TextMeshProUGUI _upgradeCostText;

    private Tower _tower;

    public bool IsActive => this.gameObject.activeSelf && _tower != null;

    private void OnEnable()
    {
        if (_tower == null)
        {
            return;
        }

        _tower.OnLevelChanged -= SetLevelText;
        _tower.OnLevelChanged += SetLevelText;

        _tower.OnAttackRateChanged -= SetSpeed;
        _tower.OnAttackRateChanged += SetSpeed;

        _tower.OnDamageChanged -= SetPower;
        _tower.OnDamageChanged += SetPower;

        _tower.OnSellCostChanged -= SetSellCost;
        _tower.OnSellCostChanged += SetSellCost;

        _tower.OnUpgradeCostChanged -= SetUpgradeCost;
        _tower.OnUpgradeCostChanged += SetUpgradeCost;
    }
    private void OnDisable()
    {
        if (_tower == null)
        {
            return;
        }

        _tower.OnLevelChanged -= SetLevelText;
        _tower.OnAttackRateChanged -= SetSpeed;
        _tower.OnDamageChanged -= SetPower;
        _tower.OnSellCostChanged -= SetSellCost;
        _tower.OnUpgradeCostChanged -= SetUpgradeCost;
    }

    // manage tower
    public void SetTower(Tower tower)
    {
        if (tower == null)
        {
            return;
        }

        // paint range
        tower.Detector.PaintRange();

        SetIcon(tower.CurrentLevelData.Icon);
        SetNameText(tower.CurrentLevelData.Name);

        SetDescriptionText(tower.CurrentLevelData.Description);
        SetLevelText(tower.CurrentLevel);
        SetPower(tower.CurrentLevelData.Damage);
        SetSpeed(tower.CurrentLevelData.AttackRate);
        SetSlow(0f); // TODO: 타워 3부터..

        SetSellCost(tower.CurrentLevelData.SellCost);
        SetUpgradeCost(tower.CurrentUpgradeCost);

        _tower = tower;
    }
    public void ClearTower()
    {
        if (_tower == null)
        {
            return;
        }

        // erase range
        _tower.Detector.EraseRange();

        SetIcon(null);
        SetNameText(string.Empty);

        SetDescriptionText(string.Empty);
        SetLevelText(0);
        SetPower(0);
        SetSpeed(0);
        SetSlow(0f); // TODO: 타워 3부터..

        SetSellCost(0);
        SetUpgradeCost(0);

        _tower = null;
    }

    // tower info - icon
    public void SetIcon(Sprite icon)
    {
        _icon.sprite = icon;
        _icon.SetNativeSize();
    }
    public void SetNameText(string name)
    {
        _nameText.text = name;
    }

    // tower info - stats
    private void SetDescriptionText(string desc)
    {
        _descriptionText.text = desc;
    }
    private void SetLevelText(int level)
    {
        _levelText.text = $"Lv.{level}";
    }
    private void SetPower(int power)
    {
        _powerText.text = power.ToString();
    }
    private void SetSpeed(float speed)
    {
        _speedText.text = speed.ToString("F1");
    }
    private void SetSlow(float slow)
    {
        _slowText.text = slow.ToString("F1");
    }

    // tower info - action
    private void SetSellCost(int cost)
    {
        _sellCostText.text = cost.ToString();
    }
    private void SetUpgradeCost(int cost)
    {
        _upgradeCostText.text = cost.ToString();
    }

    // tower action
    public void Upgrade()
    {
        _tower.Upgrade();
    }
    public void Sell()
    {
        _tower.Sell();
    }

    // panel
    public void ShowPanel(Tower tower)
    {
        SetTower(tower);

        if (_tower == null)
        {
            return;
        }

        _tower.OccupyingGridCell.ApplyGlow();
        this.gameObject.SetActive(true);

        // Upgrade UI를 보여줄 때 BuildUI를 숨김
        DefenceContext.Current.BuildUI.HidePanel();
    }
    public void HidePanel()
    {
        if (_tower == null)
        {
            return;
        }

        _tower.OccupyingGridCell.ResetGlow();
        this.gameObject.SetActive(false);

        ClearTower();

        // Upgrade UI를 숨길 때 BuildUI를 보여줌
        DefenceContext.Current.BuildUI.ShowPanel();
    }
}

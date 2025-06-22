using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    // horizontal elements
    [Header("horizontal elements")]
    [SerializeField] private GameObject _sellUI;
    [SerializeField] private GameObject _infoUI;
    [SerializeField] private GameObject _upgradeUI;

    [Space]

    // tower info - info
    [Header("tower info - info")]
    [SerializeField] [FormerlySerializedAs("_iconImage")] private Image _iconImage;
    [SerializeField] [FormerlySerializedAs("_nameText")] private TextMeshProUGUI _nameText;
    [SerializeField] [FormerlySerializedAs("_descriptionText")] private TextMeshProUGUI _descriptionText;
    [SerializeField] [FormerlySerializedAs("_levelText")] private TextMeshProUGUI _levelText;

    [Space]

    // tower info - stats
    [Header("tower info - stats")]
    [SerializeField] [FormerlySerializedAs("_attackDamageText")] private TextMeshProUGUI _attackDamageText;
    [SerializeField] [FormerlySerializedAs("_attackRateText")] private TextMeshProUGUI _attackRateText;
    [SerializeField] [FormerlySerializedAs("_slowRateText")] private TextMeshProUGUI _slowRateText;

    [Space]

    // tower info - cost
    [Header("tower info - cost")]
    [SerializeField] [FormerlySerializedAs("_sellCostText")] private TextMeshProUGUI _sellCostText;
    [SerializeField] [FormerlySerializedAs("_upgradeCostText")] private TextMeshProUGUI _upgradeCostText;

    [Space]

    [Header("ETC")]
    [SerializeField] private Sprite _normalUpgradeSprite;
    [SerializeField] private Sprite _maxUpgradeSprite;

    private Tower _tower;

    public bool IsActive => this.gameObject.activeSelf && _tower != null;

    private void OnEnable()
    {
        if (_tower == null)
        {
            return;
        }

        {
            // 업그레이드 UI를 업데이트
            SetOthers(_tower.CurrentLevel);
        }

        _tower.OnLevelChanged -= SetLevel;
        _tower.OnLevelChanged += SetLevel;

        _tower.OnLevelChanged -= SetOthers;
        _tower.OnLevelChanged += SetOthers;

        _tower.OnAttackRateChanged -= SetAttackRate;
        _tower.OnAttackRateChanged += SetAttackRate;

        _tower.OnSlowRateChanged -= SetSlowRate;
        _tower.OnSlowRateChanged += SetSlowRate;

        _tower.OnAttackDamageChanged -= SetAttackDamage;
        _tower.OnAttackDamageChanged += SetAttackDamage;

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

        _tower.OnLevelChanged -= SetLevel;
        _tower.OnLevelChanged -= SetOthers;
        _tower.OnAttackRateChanged -= SetAttackRate;
        _tower.OnAttackDamageChanged -= SetAttackDamage;
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
        SetName(tower.CurrentLevelData.Name);
        SetDescription(tower.CurrentLevelData.Description);
        SetLevel(tower.CurrentLevel);

        SetAttackDamage(tower.CurrentLevelData.AttackDamage);
        SetAttackRate(tower.CurrentLevelData.AttackRate);
        SetSlowRate(tower.CurrentLevelData.SlowRate);

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
        SetName(string.Empty);
        SetDescription(string.Empty);
        SetLevel(0);

        SetAttackDamage(0);
        SetAttackRate(0);
        SetSlowRate(0f); // TODO: 타워 3부터..

        SetSellCost(0);
        SetUpgradeCost(0);

        _tower = null;
    }

    // tower info - info
    public void SetIcon(Sprite icon)
    {
        _iconImage.sprite = icon;
        _iconImage.SetNativeSize();
    }
    public void SetName(string name)
    {
        _nameText.text = name;
    }
    private void SetDescription(string desc)
    {
        _descriptionText.text = desc;
    }
    private void SetLevel(int level)
    {
        _levelText.text = $"Lv.{level}";
    }

    /// <summary>
    /// 레벨이 변경될 때, 추가적으로 처리해줘야 하는 작업
    /// e.g) 최대 레벨 처리
    /// </summary>
    private void SetOthers(int level)
    {
        // 0. '돌'은 Upgrade Image를 비활성화
        _upgradeUI.SetActive(_tower.ID != 0 ? true : false);

        // 1. Icon 변경
        SetIcon(_tower.CurrentLevelData.Icon);

        // 2. Upgrade Image 변경
        var isMaxLevel = (level == _tower.MaxLevel);
        var upgradeImage = _upgradeUI.GetComponent<Image>();
        upgradeImage.sprite = isMaxLevel ? _maxUpgradeSprite : _normalUpgradeSprite;

        // 3. Upgrade Image의 자식들 활성화/비활성화
        void SetChildrenActive(Transform parent, bool isActive)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(isActive);
            }
        }
        SetChildrenActive(_upgradeUI.transform, !isMaxLevel);
    }

    // tower info - stats
    private void SetAttackDamage(int power)
    {
        _attackDamageText.text = power.ToString();
    }
    private void SetAttackRate(float speed)
    {
        _attackRateText.text = speed.ToString("F1");
    }
    private void SetSlowRate(float slow)
    {
        _slowRateText.text = slow.ToString("F1");
    }

    // tower info - cost
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

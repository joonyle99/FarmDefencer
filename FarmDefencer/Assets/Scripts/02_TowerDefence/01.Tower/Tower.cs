using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("���������������� Tower ����������������")]
    [Space]

    [SerializeField] private TargetableDetector _detector;

    [Space]

    [Header("Build")]
    [SerializeField] private int _cost = 10;
    public int Cost => _cost;

    [Space]

    public GameObject UpgradeUIPanel;

    [Space]

    [SerializeField] private int _maxLevel = 3;

    [SerializeField] private int _currentLevel = 1;
    public int CurrentLevel => _currentLevel;

    public int upgradeCost = 40;

    private GridCell _occupyingGridCell;

    // level 1
    public float attackRate_1 = 1f;
    public int damage_1 = 5;

    // level 2
    public float attackRate_2 = 0.8f;
    public int damage_2 = 20;

    // level 3
    public float attackRate_3 = 0.5f;
    public int damage_3 = 40;

    // Actions
    public event System.Action<int> OnLevelChanged;
    public event System.Action<float> OnAttackRateChanged;
    public event System.Action<int> OnDamageChanged;
    public event System.Action<int> OnCostChanged;

    [Space]

    [Header("Fire")]
    [SerializeField] private TowerHead _head;
    [SerializeField] private ProjectileTick _projectileTick;
    public ProjectileTick ProjectileTick => _projectileTick;
    [SerializeField] private float _currentAttackRate;
    public float CurrentAttackRate => _currentAttackRate;

    private TargetableBehavior _currentTarget;
    private float _elapsedAttackTime = 0f;

    protected override void Awake()
    {
        base.Awake();

        _currentAttackRate = attackRate_1;
    }
    private void Update()
    {
        // update target every frame
        UpdateTarget();

        if (_currentTarget != null && _currentTarget.gameObject.activeSelf == true)
        {
            _head.LookAt(_currentTarget.transform.position);
        }

        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _currentAttackRate)
        {
            _elapsedAttackTime = 0f;

            if (_currentTarget != null
                && _currentTarget.gameObject.activeSelf == true
                && _currentTarget.IsDead == false)
            {
                Attack();
            }
        }
    }

    // build
    public bool IsValidBuild(int gold)
    {
        if (gold >= _cost)
        {
            return true;
        }

        return false;
    }
    public void OccupyingGridCell(GridCell gridCell)
    {
        _occupyingGridCell = gridCell;
    }

    // fire
    private void UpdateTarget()
    {
        _currentTarget = _detector.GetFrontTarget();
    }
    private void Attack()
    {
        var projectileTick = Instantiate(_projectileTick, _head.Muzzle.position, _head.Muzzle.rotation);

        if (projectileTick == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        projectileTick.SetTarget(_currentTarget);
        projectileTick.Shoot();
    }

    // panel
    public void ShowPanel()
    {
        UpgradeUIPanel.SetActive(true);
    }
    public void HidePanel()
    {
        UpgradeUIPanel.SetActive(false);
    }

    // upgrade
    public void Upgrade()
    {
        if (_currentLevel >= _maxLevel)
        {
            Debug.Log("�ְ� �����̹Ƿ� Upgrade�� �� �����ϴ�");
            return;
        }

        if (ResourceManager.Instance.GetGold() < upgradeCost)
        {
            Debug.Log("���� �����Ͽ� Upgrade�� �� �����ϴ�");
            return;
        }

        ResourceManager.Instance.SpendGold(upgradeCost);

        _currentLevel++;
        OnLevelChanged?.Invoke(_currentLevel);

        _cost += upgradeCost;
        OnCostChanged?.Invoke(_cost);

        ReinforceRate();
        ReinforceDamage();

        HidePanel();
    }
    private void ReinforceRate()
    {
        if (_currentLevel == 1)
        {
            _currentAttackRate = attackRate_1;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
        else if (_currentLevel == 2)
        {
            _currentAttackRate = attackRate_2;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
        else if (_currentLevel == 3)
        {
            _currentAttackRate = attackRate_3;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
    }
    private void ReinforceDamage()
    {
        if (_currentLevel == 1)
        {
            _projectileTick.SetDamage(damage_1);
            OnDamageChanged?.Invoke(_projectileTick.GetDamage());
        }
        else if (_currentLevel == 2)
        {
            _projectileTick.SetDamage(damage_2);
            OnDamageChanged?.Invoke(_projectileTick.GetDamage());
        }
        else if (_currentLevel == 3)
        {
            _projectileTick.SetDamage(damage_3);
            OnDamageChanged?.Invoke(_projectileTick.GetDamage());
        }
    }

    // sell
    public void Sell()
    {
        // grid cell
        _occupyingGridCell.Usable();
        _occupyingGridCell.DeleteOccupiedTower();

        // return gold
        ResourceManager.Instance.EarnGold(_cost / 2);

        // detector
        _detector.DebugEraseRange();

        Destroy(gameObject);
    }

    public override void TakeDamage(int damage)
    {
        HP -= damage;
    }
    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}

// Ÿ���� ü��
// Ÿ���� ����

// Ÿ���� ���ݷ�
// Ÿ���� ���ݼӵ�
// Ÿ���� ���� ȿ�� (���ο�, �ߵ�, ����)

// Ÿ���� �����Ÿ� ����
// Ÿ���� Ÿ���� ���� ��
// Ÿ���� ���� ��� (����, ����, ����)
// Ÿ���� Ÿ���� ��� (���� ����� ��, ü���� ���� ��, Ȥ�� ���� ���� ��)

// Ÿ�� ��ġ ���
// Ÿ�� ��ġ ��Ÿ��

// Ÿ�� ���׷��̵�
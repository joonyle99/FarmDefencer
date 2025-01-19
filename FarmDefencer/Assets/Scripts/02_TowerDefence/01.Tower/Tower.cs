using Spine.Unity;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
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
    public int MaxLevel => _maxLevel;

    [SerializeField] private int _currentLevel = 1;
    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = value;
            
            if (_currentLevel < 1)
            {
                _currentLevel = 1;
            }
            else if (_currentLevel > 3)
            {
                _currentLevel = 3;
            }
        }
    }

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

    [Space]

    [SerializeField] private SpineController _spineController;

    [Space]

    [SpineAnimation]
    public string IdleAnimationName_1;
    [SpineAnimation]
    public string IdleAnimationName_2;
    [SpineAnimation]
    public string IdleAnimationName_3;

    [Space]

    [SpineAnimation]
    public string AttackAnimationName_1;
    [SpineAnimation]
    public string AttackAnimationName_2;
    [SpineAnimation]
    public string AttackAnimationName_3;

    [Space]

    [SpineAnimation]
    public string LevelUpAnimationName_1;
    [SpineAnimation]
    public string LevelUpAnimationName_2;

    // animation property
    [SpineAnimation]
    public string IdleAnimation
    {
        get
        {
            if (CurrentLevel == 1)
            {
                return IdleAnimationName_1;
            }
            else if (CurrentLevel == 2)
            {
                return IdleAnimationName_2;
            }
            else if (CurrentLevel == 3)
            {
                return IdleAnimationName_3;
            }

            return IdleAnimationName_1;
        }
    }
    [SpineAnimation]
    public string AttackAnimation
    {
        get
        {
            if (CurrentLevel == 1)
            {
                return AttackAnimationName_1;
            }
            else if (CurrentLevel == 2)
            {
                return AttackAnimationName_2;
            }
            else if (CurrentLevel == 3)
            {
                return AttackAnimationName_3;
            }

            return AttackAnimationName_1;
        }
    }
    [SpineAnimation]
    public string LevelUpAnimation
    {
        get
        {
            if (CurrentLevel == 1)
            {
                return LevelUpAnimationName_1;
            }
            else if (CurrentLevel == 2)
            {
                return LevelUpAnimationName_2;
            }

            return LevelUpAnimationName_1;
        }
    }

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
    protected override void OnEnable()
    {
        base.OnEnable();

        // animation
        //_spineController.SetAnimation(IdleAnimationName_1, true);
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
        // animation
        _spineController.SetAnimation(AttackAnimation, false);
        _spineController.AddAnimation(IdleAnimation, true);

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
        if (CurrentLevel >= MaxLevel)
        {
            Debug.Log("최고 레벨이므로 Upgrade할 수 없습니다");
            return;
        }

        if (ResourceManager.Instance.GetGold() < upgradeCost)
        {
            Debug.Log("돈이 부족하여 Upgrade할 수 없습니다");
            return;
        }

        ResourceManager.Instance.SpendGold(upgradeCost);

        // animation
        _spineController.SetAnimation(LevelUpAnimation, false);

        CurrentLevel++;
        OnLevelChanged?.Invoke(CurrentLevel);

        // animation
        _spineController.AddAnimation(IdleAnimation, true);

        _cost += upgradeCost;
        OnCostChanged?.Invoke(_cost);

        ReinforceRate();
        ReinforceDamage();

        HidePanel();
    }
    private void ReinforceRate()
    {
        if (CurrentLevel == 1)
        {
            _currentAttackRate = attackRate_1;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
        else if (CurrentLevel == 2)
        {
            _currentAttackRate = attackRate_2;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
        else if (CurrentLevel == 3)
        {
            _currentAttackRate = attackRate_3;
            OnAttackRateChanged?.Invoke(_currentAttackRate);
        }
    }
    private void ReinforceDamage()
    {
        if (CurrentLevel == 1)
        {
            _projectileTick.SetDamage(damage_1);
            OnDamageChanged?.Invoke(_projectileTick.GetDamage());
        }
        else if (CurrentLevel == 2)
        {
            _projectileTick.SetDamage(damage_2);
            OnDamageChanged?.Invoke(_projectileTick.GetDamage());
        }
        else if (CurrentLevel == 3)
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

// 타워의 체력
// 타워의 방어력

// 타워의 공격력
// 타워의 공격속도
// 타워의 공격 효과 (슬로우, 중독, 기절)

// 타워의 사정거리 범위
// 타워의 타게팅 가능 수
// 타워의 공격 대상 (지상, 공중, 수중)
// 타워의 타게팅 방식 (가장 가까운 적, 체력이 적은 적, 혹은 가장 빠른 적)

// 타워 설치 비용
// 타워 설치 쿨타임

// 타워 업그레이드
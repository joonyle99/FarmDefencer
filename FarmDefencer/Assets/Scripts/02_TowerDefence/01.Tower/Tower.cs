using Spine.Unity;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
    [Space]

    [SerializeField] private TargetableDetector _detector;

    [Space]

    [SerializeField] private string _towerName;
    public string TowerName => _towerName;

    [Space]

    [Header("Build")]
    [SerializeField] private int _cost = 10;
    public int Cost => _cost;

    private GridCell _occupyingGridCell;

    [Space]

    // TODO: Tower Stats -> ScriptableObject로 변경하기

    [Header("Upgrade")]
    public UpgradePanel UpgradeUIPanel;

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

    [Space]

    public int upgradeCost_1_2 = 30;
    public int upgradeCost_2_3 = 40;

    [Space]

    // level 1
    public float attackRate_1 = 1f;
    public int damage_1 = 5;
    public int sellCost_1 = 5;

    [Space]

    // level 2
    public float attackRate_2 = 0.8f;
    public int damage_2 = 20;
    public int sellCost_2 = 37;

    [Space]

    // level 3
    public float attackRate_3 = 0.5f;
    public int damage_3 = 40;
    public int sellCost_3 = 89;

    [Space]

    [Space]

    [Header("Animation")]
    [SerializeField] private SpineController _spineController;

    [Space]

    [SpineAnimation]
    public string IdleAnimationName_1;
    [SpineAnimation]
    public string IdleAnimationName_2;
    [SpineAnimation]
    public string IdleAnimationName_3;
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

    [Space]

    [SpineAnimation]
    public string AttackAnimationName_1;
    [SpineAnimation]
    public string AttackAnimationName_2;
    [SpineAnimation]
    public string AttackAnimationName_3;
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

    [Space]

    [SpineAnimation]
    public string LevelUpAnimationName_1;
    [SpineAnimation]
    public string LevelUpAnimationName_2;
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

    [Space]

    [Header("Fire")]
    [SerializeField] private TowerHead _head; // TODO: TowerHead의 하위 클래스 생성하기
    public TowerHead Head => _head;
    [SerializeField] private ProjectileTick _projectileTickPrefab; // TODO: ProjectileTick의 하위 클래스 생성하기
    public ProjectileTick ProjectileTickPrefab => _projectileTickPrefab;

    private float _currentAttackRate;
    public float CurrentAttackRate => _currentAttackRate;
    private int _currentDamage;
    public int CurrentDamage => _currentDamage;

    protected TargetableBehavior currentTarget;
    private float _attackWaitTimer = 0f;

    private bool _isAttacking = false;
    private float _attackAnimDuration_1;
    private float _attackAnimDuration_2;
    private float _attackAnimDuration_3;
    private float _attackElapsedTime = 0f;

    // Actions
    public event System.Action<int> OnLevelChanged;
    public event System.Action<float> OnAttackRateChanged;
    public event System.Action<int> OnDamageChanged;
    public event System.Action<int> OnCostChanged;

    protected override void Awake()
    {
        base.Awake();

        _currentAttackRate = attackRate_1;
        _currentDamage = damage_1;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Start()
    {
        base.Start();

        UpgradeUIPanel.SetOwner(this);

        _attackAnimDuration_1 = _spineController.Skeleton.Data.FindAnimation(AttackAnimationName_1).Duration;
        _attackAnimDuration_2 = _spineController.Skeleton.Data.FindAnimation(AttackAnimationName_2).Duration;
        _attackAnimDuration_3 = _spineController.Skeleton.Data.FindAnimation(AttackAnimationName_3).Duration;
    }
    private void Update()
    {
        if (_isAttacking == true)
        {
            _attackElapsedTime += Time.deltaTime;

            if (CurrentLevel == 1)
            {
                if (_attackElapsedTime >= _attackAnimDuration_1)
                {
                    _attackElapsedTime = 0f;
                    _isAttacking = false;
                }
            }
            else if (CurrentLevel == 2)
            {
                if (_attackElapsedTime >= _attackAnimDuration_2)
                {
                    _attackElapsedTime = 0f;
                    _isAttacking = false;
                }
            }
            else if (CurrentLevel == 3)
            {
                if (_attackElapsedTime >= _attackAnimDuration_3)
                {
                    _attackElapsedTime = 0f;
                    _isAttacking = false;
                }
            }
        }

        // find
        if (_isAttacking == false)
        {
            UpdateTarget();
            if (currentTarget != null && currentTarget.gameObject.activeSelf == true)
            {
                _head.LookAt(currentTarget.transform.position);
            }
        }

        // attack
        _attackWaitTimer += Time.deltaTime;
        if (_attackWaitTimer >= _currentAttackRate)
        {
            _attackWaitTimer = 0f;

            if (currentTarget != null
                && currentTarget.gameObject.activeSelf == true
                && currentTarget.IsDead == false)
            {
                Attack();
            }
        }

        // TODO
        // 현재 문제점..
        // 공격할 때 LooAt을 한다
        // 최소한 공격 애니메이션은 끝나고 방향을 전환해야 한다
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
        currentTarget = _detector.GetFrontTarget();
    }
    private void Attack()
    {
        _isAttacking = true;

        // animation
        _spineController.SetAnimation(AttackAnimation, false);
        _spineController.AddAnimation(IdleAnimation, true);

        ShootingProcess();
    }
    protected virtual void ShootingProcess()
    {
        // 공격 애니메이션은 바로 실행된다
        // 그러므로 공격 애니메이션의 시간을 알아내어
        // 그 시간 동안 대기 시간을 둔다

        var projectileTick = Instantiate(_projectileTickPrefab, _head.Muzzle.position, _head.Muzzle.rotation);

        if (projectileTick == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        projectileTick.SetTarget(currentTarget);
        projectileTick.SetDamage(_currentDamage);
        projectileTick.Shoot();

        SoundManager.Instance.PlaySfx($"SFX_D_turretShot_1-{CurrentLevel}");
    }

    // panel
    public void ShowPanel()
    {
        UpgradeUIPanel.gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        UpgradeUIPanel.gameObject.SetActive(false);
    }

    // upgrade
    public void Upgrade()
    {
        if (CurrentLevel >= MaxLevel)
        {
            Debug.Log("최고 레벨이므로 Upgrade할 수 없습니다");
            return;
        }

        // cost 1
        var upgradeCost = int.MaxValue;
        if (CurrentLevel == 1) upgradeCost = upgradeCost_1_2;
        else if (CurrentLevel == 2) upgradeCost = upgradeCost_2_3;
        if (ResourceManager.Instance.GetGold() < upgradeCost)
        {
            Debug.Log("돈이 부족하여 Upgrade할 수 없습니다");
            return;
        }
        ResourceManager.Instance.SpendGold(upgradeCost);

        // animation
        _spineController.SetAnimation(LevelUpAnimation, false);

        // level
        CurrentLevel++;
        OnLevelChanged?.Invoke(CurrentLevel);

        // animation
        _spineController.AddAnimation(IdleAnimation, true);

        // cost 2
        _cost += upgradeCost;
        OnCostChanged?.Invoke(_cost);

        Reinforce();

        HidePanel();
    }
    private void Reinforce()
    {
        ReinforceRate();
        ReinforceDamage();

        SoundManager.Instance.PlaySfx("SFX_D_turret_levelup");
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
            _currentDamage = damage_1;
            OnDamageChanged?.Invoke(_currentDamage);
        }
        else if (CurrentLevel == 2)
        {
            _currentDamage = damage_2;
            OnDamageChanged?.Invoke(_currentDamage);
        }
        else if (CurrentLevel == 3)
        {
            _currentDamage = damage_3;
            OnDamageChanged?.Invoke(_currentDamage);
        }
    }

    // sell
    public void Sell()
    {
        // grid cell
        _occupyingGridCell.Usable();
        _occupyingGridCell.DeleteOccupiedTower();

        // TODO: sellCost 변수 하나로 줄이기
        // return gold
        var sellCost = 0;
        if (CurrentLevel == 1)
        {
            sellCost = sellCost_1;
        }
        else if (CurrentLevel == 2)
        {
            sellCost = sellCost_2;
        }
        else if (CurrentLevel == 3)
        {
            sellCost = sellCost_3;
        }

        ResourceManager.Instance.EarnGold(sellCost);

        SoundManager.Instance.PlaySfx("SFX_D_turret_remove");

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
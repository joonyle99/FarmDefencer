using Spine.Unity;
using System;
using UnityEngine;

/// <summary>
///
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
    [Space]

    [Header("Module")]
    [SerializeField] private TargetableDetector _detector;
    public TargetableDetector Detector => _detector;

    [Header("Default")]
    [SerializeField] private int _id = 1;
    private int _level = 1;
    private int _cost = 0;
    public int ID => _id;
    public int CurrentLevel
    {
        get => _level;
        set
        {
            _level = Mathf.Clamp(value, 1, MaxLevel);
            OnLevelChanged?.Invoke(_level);
        }
    }
    public int MaxLevel => _levelData.Length;
    public int CurrentCost
    {
        get => _cost;
        set
        {
            _cost = Mathf.Max(value, 0);
            OnCostChanged?.Invoke(_cost);
            OnSellCostChanged?.Invoke(CurrentLevelData.SellCost);
            OnUpgradeCostChanged?.Invoke(CurrentUpgradeCost);
        }
    }
    public int CurrentUpgradeCost
    {
        get
        {
            var nextValueCost = _levelData[Mathf.Min(CurrentLevel, MaxLevel - 1)].ValueCost;
            var currValueCost = CurrentLevelData.ValueCost;
            return nextValueCost - currValueCost;
        }
    }

    [Header("Stats")]
    [SerializeField] private TowerLevelData[] _levelData;
    public TowerLevelData[] LevelData => _levelData;
    public TowerLevelData DefaultLevelData => _levelData[0];
    public TowerLevelData CurrentLevelData => _levelData[CurrentLevel - 1];

    [Header("Animation")]
    [SpineAnimation] public string[] IdleAnimationNames;
    [SpineAnimation] public string[] AttackAnimationNames;
    [SpineAnimation] public string[] LevelUpAnimationNames;
    public string IdleAnimation => IdleAnimationNames[CurrentLevel - 1];
    public string AttackAnimation => AttackAnimationNames[CurrentLevel - 1];
    public string LevelUpAnimation => LevelUpAnimationNames[CurrentLevel - 1];
    private float[] _attackDuration;
    public float AttackDuration => _attackDuration[CurrentLevel - 1];
    private bool _isAttacking = false;
    private float _attackCooldownTimer = 0f;
    private float _attackDurationTimer = 0f;

    [Header("Fire")]
    [SerializeField] private FlipAimer _flipAimer;
    public FlipAimer FlipAimer => _flipAimer;
    [SerializeField] private GameObject _projectilePrefab;

    // build
    private GridCell _occupyingGridCell;
    public GridCell OccupyingGridCell => _occupyingGridCell;

    // target
    private TargetableBehavior _currentTarget;
    public TargetableBehavior CurrentTarget => _currentTarget;
    public bool IsValidTarget => _currentTarget != null && _currentTarget.gameObject.activeSelf;
    public bool IsAttackableTarget => IsValidTarget && !_currentTarget.IsDead;

    // actions
    public event System.Action<int> OnLevelChanged;
    public event System.Action<float> OnAttackRateChanged;
    public event System.Action<float> OnSlowRateChanged;
    public event System.Action<int> OnDamageChanged;
    public event System.Action<int> OnCostChanged;
    public event System.Action<int> OnSellCostChanged;
    public event System.Action<int> OnUpgradeCostChanged;

    protected override void Awake()
    {
        base.Awake();

        _cost = CurrentLevelData.ValueCost;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Start()
    {
        base.Start();

        InitializeAttackDurations();
        InitilaizeSpineEvent();
    }
    private void Update()
    {
        if (AttackAnimationNames.Length > 0 && string.IsNullOrEmpty(AttackAnimation) == false)
        {
            UpdateAttackState();
        }
    }

    // initialize
    private void InitializeAttackDurations()
    {
        var skeletonData = spineController.Skeleton.Data;
        _attackDuration = new float[AttackAnimationNames.Length];

        for (int i = 0; i < _attackDuration.Length; i++)
        {
            var animation = skeletonData.FindAnimation(AttackAnimationNames[i]);
            _attackDuration[i] = (animation != null) ? animation.Duration : 0f;
        }
    }
    private void InitilaizeSpineEvent()
    {
        spineController.SkeletonAnimation.AnimationState.Event += HandleSpineEvent;
    }
    private void HandleSpineEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "fire":
            case "shoot":
                Fire();
                break;
        }
    }

    // build
    public bool HasEnoughGold()
    {
        var currentGold = ResourceManager.Instance.GetGold();
        if (currentGold >= CurrentCost)
        {
            return true;
        }

        return false;
    }
    public void OccupyGridCell(GridCell gridCell)
    {
        _occupyingGridCell = gridCell;
    }

    // sell
    public void Sell()
    {
        // grid cell
        _occupyingGridCell.Usable();
        _occupyingGridCell.DeleteOccupiedTower();

        ResourceManager.Instance.EarnGold(CurrentLevelData.SellCost);
        SoundManager.Instance.PlaySfx("SFX_D_tower_remove");

        // detector
        Detector.EraseRange();

        var upgradeUI = DefenceContext.Current.UpgradeUI;
        upgradeUI.HidePanel();
        upgradeUI.ClearTower();

        Destroy(gameObject);
    }

    // upgrade
    public void Upgrade()
    {
        if (CurrentLevel >= MaxLevel)
        {
            Debug.Log("최고 레벨이므로 타워를 업그레이드 할 수 없습니다");
            return;
        }

        var result = ResourceManager.Instance.TrySpendGold(CurrentUpgradeCost);
        if (result == false)
        {
            Debug.Log("골드가 부족하여 타워를 업그레이드 할 수 없습니다");
            return;
        }

        // prev level animation
        spineController.SetAnimation(LevelUpAnimation, false);

        // level up
        CurrentLevel += 1;
        CurrentCost = CurrentLevelData.ValueCost;

        // next level animation
        spineController.AddAnimation(IdleAnimation, true); // 레벨이 오른 후의 애니메이션을 출력해야 한다

        // var upgradeUI = DefenceContext.Current.UpgradeUI;
        // upgradeUI.HidePanel();
        // upgradeUI.ClearTower();

        Reinforce();
    }
    private void Reinforce()
    {
        OnAttackRateChanged?.Invoke(CurrentLevelData.AttackRate);
        OnSlowRateChanged?.Invoke(CurrentLevelData.SlowRate);
        OnDamageChanged?.Invoke(CurrentLevelData.Damage);

        SoundManager.Instance.PlaySfx("SFX_D_tower_upgrade");
    }

    // fire
    private void UpdateAttackState()
    {
        // wait attack duration
        if (_isAttacking == true)
        {
            _attackDurationTimer += Time.deltaTime;
            if (_attackDurationTimer >= AttackDuration)
            {
                _attackDurationTimer = 0f;
                _isAttacking = false;
            }
        }

        // find
        if (_isAttacking == false) // 공격 애니메이션 재생 중에 다른 곳을 바라보지 않도록 하기 위함
        {
            UpdateTarget();
        }

        // attack
        _attackCooldownTimer += Time.deltaTime; // 공격 쿨타임은 공격 애니메이션 재생 중에도 증가
        if (_attackCooldownTimer >= CurrentLevelData.AttackRate)
        {
            _attackCooldownTimer = 0f;
            if (IsAttackableTarget)
            {
                Attack();
            }
        }
    }
    private void UpdateTarget()
    {
        _currentTarget = Detector.GetFrontTarget();
    }
    private void Attack()
    {
        _isAttacking = true;

        // animation
        spineController.SetAnimation(AttackAnimation, false);
        spineController.AddAnimation(IdleAnimation, true);
    }
    private void Fire()
    {
        if (!IsValidTarget)
        {
            return;
        }

        _flipAimer.FlipAim(CurrentTarget.transform.position);

        var projectileGO = Instantiate(_projectilePrefab, spineController.GetShootingBonePos(CurrentLevel), Quaternion.identity);
        var projectile = projectileGO.GetComponent<ProjectileBase>();

        if (projectile == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        projectile.SetTarget(CurrentTarget);
        projectile.SetDamage(CurrentLevelData.Damage);
        projectile.SetSlow(CurrentLevelData.SlowRate, CurrentLevelData.SlowDuration);
        projectile.Trigger();

        SoundManager.Instance.PlaySfx($"SFX_D_tower_shot_{ID}-{CurrentLevel}");
    }

    // hit
    public override void TakeDamage(int damage, ProjectileType type)
    {
        HP -= damage;
    }
    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}

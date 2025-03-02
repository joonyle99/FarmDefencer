using Spine.Unity;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
    [Space]

    [Header("Module")]
    [SerializeField] private TargetableDetector _detector;

    [Space]

    [Header("Level Data")]
    [SerializeField] private TowerLevelData[] _levelData;
    private TowerLevelData _currentLevelData;
    public TowerLevelData CurrentLevelData
    {
        get
        {
            _currentLevelData = _levelData[CurrentLevel - 1];
            return _currentLevelData;
        }
    }

    [Space]

    [Header("Default")]
    [SerializeField] private string _towerName;
    [SerializeField][Tooltip("default cost at first, after based on upgradeCost")] private int _cost;
    [SerializeField][Tooltip("tower current level linked levelData")] private int _level = 1;

    public string TowerName => _towerName;
    public int CurrentCost
    {
        get => _cost;
        set
        {
            _cost = Mathf.Max(value, 0);
            OnCostChanged?.Invoke(_cost); // TODO: 변화한 경우에만 호출..?
        }
    }
    public int CurrentLevel
    {
        get => _level;
        set
        {
            _level = Mathf.Clamp(value, 1, MaxLevel);
            OnLevelChanged?.Invoke(_level); // TODO: 변화한 경우에만 호출..?
        }
    }

    public int MaxLevel => _levelData.Length;

    [Space]

    [Header("Animation")]
    [SerializeField] private SpineController _spineController;

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
    [SerializeField] private TowerHead _head; // TODO: TowerHead의 하위 클래스 생성하기
    [SerializeField] private ProjectileTick _projectileTickPrefab; // TODO: ProjectileTick의 하위 클래스 생성하기

    public TowerHead Head => _head;
    public ProjectileTick ProjectileTickPrefab => _projectileTickPrefab;

    [Space]

    [Header("UI")]
    public UpgradePanel UpgradePanel;

    // build
    private GridCell _occupyingGridCell;

    // target
    private TargetableBehavior _currentTarget;
    public TargetableBehavior CurrentTarget => _currentTarget;
    public bool IsValidTarget => _currentTarget != null && _currentTarget.gameObject.activeSelf;
    public bool IsAttackableTarget => IsValidTarget && !_currentTarget.IsDead;

    // actions
    public event System.Action<int> OnLevelChanged;
    public event System.Action<float> OnAttackRateChanged;
    public event System.Action<int> OnDamageChanged;
    public event System.Action<int> OnCostChanged;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Start()
    {
        base.Start();
        UpgradePanel.SetOwner(this);
        InitializeAttackDurations();
        InitilaizeSpineEvent();
    }
    private void Update()
    {
        UpdateAttackState();
    }

    // initialize
    private void InitializeAttackDurations()
    {
        var skeletonData = _spineController.Skeleton.Data;
        _attackDuration = new float[AttackAnimationNames.Length];

        for (int i = 0; i < _attackDuration.Length; i++)
        {
            var animation = skeletonData.FindAnimation(AttackAnimationNames[i]);
            _attackDuration[i] = (animation != null) ? animation.Duration : 0f;
        }
    }
    private void InitilaizeSpineEvent()
    {
        _spineController.SkeletonAnimation.AnimationState.Event += HandleSpineEvent;
    }
    private void HandleSpineEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "fire":
                Shoot();
                break;
        }
    }

    // build
    public bool IsValidBuild(int gold)
    {
        if (gold >= CurrentCost)
        {
            return true;
        }

        return false;
    }
    public void OccupyingGridCell(GridCell gridCell)
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
        SoundManager.Instance.PlaySfx("SFX_D_turret_remove");

        // detector
        _detector.DebugEraseRange();

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

        var result = ResourceManager.Instance.TrySpendGold(CurrentLevelData.UpgradeCost);
        if (result == false)
        {
            Debug.Log("골드가 부족하여 타워를 업그레이드 할 수 없습니다");
            return;
        }

        // prev level animation
        _spineController.SetAnimation(LevelUpAnimation, false);

        // level up
        CurrentCost += CurrentLevelData.UpgradeCost;
        CurrentLevel += 1;

        // next level animation
        _spineController.AddAnimation(IdleAnimation, true); // 레벨이 오른 후의 애니메이션을 출력해야 한다

        Reinforce();

        HideUpgradePanel();
    }
    private void Reinforce()
    {
        OnAttackRateChanged?.Invoke(CurrentLevelData.AttackRate);
        OnDamageChanged?.Invoke(CurrentLevelData.Damage);

        SoundManager.Instance.PlaySfx("SFX_D_turret_levelup");
    }
    public void ShowUpgradePanel()
    {
        UpgradePanel.gameObject.SetActive(true);
    }
    public void HideUpgradePanel()
    {
        UpgradePanel.gameObject.SetActive(false);
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
            if (IsValidTarget)
            {
                _head.LookAt(CurrentTarget.transform.position);
            }
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
        _currentTarget = _detector.GetFrontTarget();
    }
    private void Attack()
    {
        _isAttacking = true;

        // animation
        _spineController.SetAnimation(AttackAnimation, false);
        _spineController.AddAnimation(IdleAnimation, true);
    }
    protected virtual void Shoot()
    {
        var projectileTick = Instantiate(_projectileTickPrefab, Head.Muzzle.position, Head.Muzzle.rotation);

        if (projectileTick == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        projectileTick.SetTarget(CurrentTarget);
        projectileTick.SetDamage(CurrentLevelData.Damage);
        projectileTick.Shoot();

        SoundManager.Instance.PlaySfx($"SFX_D_turretShot_1-{CurrentLevel}");
    }

    // hit
    public override void TakeDamage(int damage)
    {
        HP -= damage;
    }
    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}

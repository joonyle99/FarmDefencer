using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using UnityEngine;

/// <summary>
///
/// </summary>
public sealed class Tower : TargetableBehavior, IVolumeControl
{
    #region Attributes

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
        get
        {
            if (ID == 0)
            {
                var mapIndex = MapManager.Instance.CurrentMapIndex;
                var level = Mathf.Clamp(mapIndex, 1, MaxLevel);
                _level = level;
                return _level;
            }

            return _level;
        }
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

    [Header("Data")]
    [SerializeField] private TowerLevelData[] _levelData;
    public TowerLevelData[] LevelData => _levelData;
    public TowerLevelData DefaultLevelData
    {
        get
        {
            if (ID == 0)
            {
                var mapIndex = MapManager.Instance.CurrentMapIndex;
                var level = Mathf.Clamp(mapIndex, 1, MaxLevel);
                var levelIndex = Mathf.Clamp(level - 1, 0, MaxLevel - 1);
                return _levelData[levelIndex];
            }

            return _levelData[0];
        }
    }
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
    private float _attackCooldownTimer = 0f;

    [Header("Fire")]
    [SerializeField] private bool _canAttack = true;
    private bool _isAttacking = false;
    [SerializeField] private FlipAimer _flipAimer;
    public FlipAimer FlipAimer => _flipAimer;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private GameObject _beamPrefab;

    // build
    private GridCell _occupyingGridCell;
    public GridCell OccupyingGridCell => _occupyingGridCell;

    // target
    private TargetableBehavior _currentTarget;
    public TargetableBehavior CurrentTarget => _currentTarget;
    public bool HasValidTarget => _currentTarget != null && _currentTarget.gameObject.activeSelf;
    public bool HasAttackableTarget => HasValidTarget && !_currentTarget.IsDead;

    // actions
    public event System.Action<int> OnLevelChanged;
    public event System.Action<int> OnAttackDamageChanged;
    public event System.Action<float> OnAttackRateChanged;
    public event System.Action<float> OnSlowRateChanged;
    public event System.Action<int> OnCostChanged;
    public event System.Action<int> OnSellCostChanged;
    public event System.Action<int> OnUpgradeCostChanged;

    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float removeVolume = 0.5f;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float upgradeVolume = 0.5f;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float fireReadyVolume = 0.5f;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float fireShotVolume = 0.5f;

    #endregion

    #region Functions

    protected override void Awake()
    {
        base.Awake();

        _cost = CurrentLevelData.ValueCost;

        // 둘 다 null이거나 둘 다 할당된 경우
        bool isProjectileAssigned = _projectilePrefab != null;
        bool isBeamAssigned = _beamPrefab != null;
        if (isProjectileAssigned == true && isBeamAssigned == true)
        {
            Debug.LogError("_projectilePrefab 또는 _beamPrefab 중 하나만 할당하세요.");
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        OnLevelChanged -= InitAttackDurations;
        OnLevelChanged += InitAttackDurations;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        OnLevelChanged -= InitAttackDurations;
        spineController.SkeletonAnimation.AnimationState.Event -= HandleSpineEvent;
    }
    protected override void Start()
    {
        base.Start();

        InitAttackDurations();

        // TODO: 이후에 Try 함수를 만들어서 OnEnable, Start, Setter 등 사용하는 곳에서 호출해주어 구독을 빼먹지 않도록 한다..
        spineController.SkeletonAnimation.AnimationState.Event -= HandleSpineEvent;
        spineController.SkeletonAnimation.AnimationState.Event += HandleSpineEvent;

        spineController.SetAnimation(IdleAnimation, true);
    }
    private void Update()
    {
        if (isActivated == false)
        {
            return;
        }

        if (AttackAnimationNames.Length > 0 && string.IsNullOrEmpty(AttackAnimation) == false)
        {
            UpdateAttackState();
        }
    }

    // initialize
    private void InitAttackDurations(int _ = 0)
    {
        var skeletonData = spineController.Skeleton.Data;
        _attackDuration = new float[AttackAnimationNames.Length];

        for (int i = 0; i < _attackDuration.Length; i++)
        {
            var animation = skeletonData.FindAnimation(AttackAnimationNames[i]);
            if (animation != null)
            {
                var eventTime = 0f;

                foreach (var timeline in animation.Timelines)
                {
                    if (timeline is Spine.EventTimeline eventTimeline)
                    {
                        for (int j = 0; j < eventTimeline.Events.Length; j++)
                        {
                            var evt = eventTimeline.Events[j];
                            if (evt.Data.Name == "fire" || evt.Data.Name == "shoot")
                            {
                                eventTime = evt.Time;
                                break;
                            }
                        }
                    }
                }

                if (Mathf.Approximately(eventTime, 0f))
                {
                    eventTime = animation.Duration;
                }

                _attackDuration[i] = eventTime;
            }
            else
            {
                _attackDuration[i] = 0f;
            }
        }
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
    public bool EnoughGold()
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
        SoundManager.Instance.PlaySfx("SFX_D_tower_remove", removeVolume);

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
        // check stone (not tower, can not upgrade)
        if (ID == 0)
        {
            return;
        }

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
        OnAttackDamageChanged?.Invoke(CurrentLevelData.AttackDamage);
        OnAttackRateChanged?.Invoke(CurrentLevelData.AttackRate);
        OnSlowRateChanged?.Invoke(CurrentLevelData.SlowRate);

        SoundManager.Instance.PlaySfx("SFX_D_tower_upgrade", upgradeVolume);
    }

    // fire
    private void UpdateAttackState()
    {
        // check can attack
        if (_canAttack == false)
        {
            return;
        }

        // find
        if (_isAttacking == false) // 공격 애니메이션 재생 중에 다른 곳을 바라보지 않도록 하기 위함
        {
            UpdateTarget();
        }

        // attack
        _attackCooldownTimer += Time.deltaTime; // 공격 쿨타임은 공격 애니메이션 재생 중에도 증가
        if (_attackCooldownTimer >= CurrentLevelData.AttackRate && _attackCooldownTimer >= AttackDuration)
        {
            if (HasAttackableTarget)
            {
                _attackCooldownTimer = 0f;
                Attack();
            }
        }
    }
    private void UpdateTarget()
    {
        if (_projectilePrefab != null)
        {
            _currentTarget = Detector.GetFrontTarget();
        }
        else if (_beamPrefab != null)
        {
            _currentTarget = Detector.GetBackTarget();
        }
    }
    private void Attack()
    {
        _isAttacking = true;

        // animation
        spineController.SetAnimation(AttackAnimation, false);
        spineController.AddAnimation(IdleAnimation, true);

        // sound
        if (CurrentLevelData.FireReady != null)
        {
            SoundManager.Instance.PlaySfx(CurrentLevelData.FireReady, fireReadyVolume);
        }
    }
    private void Fire()
    {
        _isAttacking = false;

        if (!HasValidTarget)
        {
            return;
        }

        _flipAimer.FlipAim(CurrentTarget.transform.position);

        if (_projectilePrefab != null)
        {
            var projectileGO = Instantiate(_projectilePrefab, spineController.GetShootingBonePos(CurrentLevel), Quaternion.identity);
            var projectile = projectileGO.GetComponent<ProjectileBase>();

            if (projectile == null)
            {
                Debug.LogWarning($"projectile is null");
                return;
            }

            projectile.SetCaster(this);
            projectile.SetTarget(CurrentTarget);
            projectile.SetDamage(CurrentLevelData.AttackDamage);
            projectile.SetSlow(CurrentLevelData.SlowRate, CurrentLevelData.SlowDuration);
            projectile.Trigger();
        }
        else if (_beamPrefab != null)
        {
            var beamGo = Instantiate(_beamPrefab, spineController.GetShootingBonePos(CurrentLevel), Quaternion.identity);
            var beam = beamGo.GetComponent<BeamBase>();

            if (beam == null)
            {
                Debug.LogWarning($"beam is null");
                return;
            }

            // beam.SetCaster(this);
            beam.SetTarget(CurrentTarget);
            beam.SetDamage(CurrentLevelData.AttackDamage);
            beam.SetSlow(CurrentLevelData.SlowRate, CurrentLevelData.SlowDuration);
            beam.SetTower(this);
            beam.SetStayDuration(CurrentLevelData.StayDuration);
            beam.SetDealInterval(CurrentLevelData.DealInterval);
            beam.Trigger();
        }

        // sound
        if (CurrentLevelData.FireShot != null)
        {
            SoundManager.Instance.PlaySfx(CurrentLevelData.FireShot, fireShotVolume);
        }
    }

    // hit
    public override void TakeDamage(int damage, DamageType type)
    {
        HP -= damage;
    }
    public override void Kill()
    {
        throw new System.NotImplementedException();
    }

    #endregion
}

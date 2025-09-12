using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

/// <summary>
///
/// </summary>
public class Monster : TargetableBehavior, IProduct, IVolumeControl
{
    #region Attributes

    [Header("──────── IProduct ────────")]
    [Space]

    [Header("Factory")]
    [SerializeField] private Factory _originFactory;
    public Factory OriginFactory
    {
        get => _originFactory;
        set => _originFactory = value;
    }

    public void SetOriginFactory(Factory originFactory)
    {
        _originFactory = originFactory;
    }

    public GameObject GameObject => this.gameObject;
    public Transform Transform => this.transform;

    [Header("──────── Monster ────────")]
    [Space]

    [Header("Data")]
    [SerializeField] private MonsterData _monsterData;
    public MonsterData MonsterData => _monsterData;

    [Space]

    [Header("Animation")]
    [SpineAnimation] public string IdleAnimationName;
    [SpineAnimation] public string WalkAnimationName;
    [SpineAnimation] public string WalkDamagedAnimationName;
    [SpineAnimation] public string DissappearAnimationName;

    // events
    public event System.Action<int> OnDamaged;
    public event System.Action<Monster> OnKilled;
    public event System.Action<Monster> OnSurvived;

    // sorting
    private SortingGroup _sortingGroup;
    private int _defaultSortingOrder;

    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float deadVolume = 0.5f;

    #endregion

    #region Functions

    protected override void Awake()
    {
        base.Awake();

        _sortingGroup = GetComponent<SortingGroup>();
        _defaultSortingOrder = _sortingGroup.sortingOrder;

        InstantTransparent();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        maxHp = _monsterData.MaxHp;

        InstantOpaque();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        // stop all coroutines
        // 1. TransparentCo
        // 2. OpaqueCo
        // 3. KillCo
        // 4. SurviveCo
        // 5. StunCo

        StopAllCoroutines();

        OnDamaged = null;
        OnKilled = null;
        OnSurvived = null;
    }
    protected override void Start()
    {
        base.Start();

        // Debug.Log("Monster Start()");
    }
    private void Update()
    {
        if (DefenceContext.Current is null)
        {
            return;
        }

        var dirX = DefenceContext.Current.GridMap.DirectionToEndX; // 1 or -1

        // cf)
        // transform.position.x: 0 ~ 17
        // transform.position.y: 0 ~ 10

        // 1. 기본적으로 y좌표가 낮을 수록 레이어가 높아야 한다
        // 2. 기본적으로 x좌표가 높을 수록 레이어가 높아야 한다

        // 위 조건을 기준으로 측정된 계산.. dirX / dirY에 따라 계산이 달라져야 한다
        // 어떠한 경우이냐면,, dirX가 -1인 경우에는 왼쪽에 위치한 오브젝트의 레이어가 더 높아야한다
        var weightY = 100; var weightX = 10; // y좌표에 대한 가중치 >>> x좌표에 대한 가중치
        var deltaY = -1 * (int)Mathf.Floor(transform.position.y * weightY); // y좌표에 따른 레이어 순서는 절대적이어야 한다
        var deltaX = dirX * (int)Mathf.Floor(transform.position.x * weightX); // x좌표에 다른 레이어 순서는 dirX에 따라 달라져야 한다
        _sortingGroup.sortingOrder = _defaultSortingOrder + deltaY + deltaX;
    }

    public override void TakeDamage(int damage, DamageType type)
    {
        if (IsDead == true)
        {
            return;
        }

        HP -= damage;

        var damageTextPrefab = AssetCache.Get<DamageText>("DamageText");
        var damageTextGo = Instantiate(damageTextPrefab, Vector3.zero, Quaternion.identity);
        damageTextGo.Init(damage.ToString(), type, HeadPoint);

        if (HP > 0)
        {
            // 현재 실행 중인 애니메이션 확인
            var currentAnimation = spineController.SpineAnimationState.GetCurrent(0);
            if (currentAnimation != null
                && currentAnimation.Animation.Name != WalkDamagedAnimationName)
            {
                spineController.SetAnimation(WalkDamagedAnimationName, false);
                spineController.AddAnimation(WalkAnimationName, true);
            }

            //StartCoroutine(StunCo(StunDuration));

            // 여기서,,
        }

        OnDamaged?.Invoke(damage);
    }
    public override void Kill()
    {
        IsDead = true;

        gridMovement.Rigidbody.linearVelocity = Vector2.zero;

        SoundManager.Instance.PlaySfx($"SFX_D_{MonsterData.Name}_dead", deadVolume);

        StartCoroutine(KillCo(DissappearAnimationName));

        /*
        SetAnimation(DissappearAnimationName, false);

        //_spineAnimationState.Complete -= OnDissapearComplete_Killed;
        //_spineAnimationState.Complete += OnDissapearComplete_Killed;
        */
    }

    public void Survive()
    {
        IsDead = true;

        gridMovement.Rigidbody.linearVelocity = Vector2.zero;

        StartCoroutine(SurviveCo());

        /*
        SetAnimation(DissappearAnimationName, false);

        //_spineAnimationState.Complete -= OnDissapearComplete_Survive;
        //_spineAnimationState.Complete += OnDissapearComplete_Survive;
        */
    }

    // color alpha
    private void InstantTransparent()
    {
        var t = Mathf.Clamp01(0f);

        spineController.Skeleton.A = t;
        HealthBar?.SetAlpha(t);
    }
    private void InstantOpaque()
    {
        var t = Mathf.Clamp01(1f);

        spineController.Skeleton.A = t;
        HealthBar?.SetAlpha(t);
    }
    private IEnumerator TransparentCo(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;
            t = Mathf.Clamp01(1f - t);

            spineController.Skeleton.A = t;
            HealthBar?.SetAlpha(t);

            yield return null;

            eTime += Time.deltaTime;
        }

        InstantTransparent();
    }
    private IEnumerator OpaqueCo(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;

            spineController.Skeleton.A = Mathf.Clamp01(t);

            yield return null;

            eTime += Time.deltaTime;
        }

        InstantOpaque();
    }

    private IEnumerator KillCo(string animationName = "None")
    {
        if (animationName != "None")
        {
            spineController.SetAnimation(animationName, false);

            var animation = spineController.Skeleton.Data.FindAnimation(animationName);
            if (animation != null)
            {
                yield return new WaitForSeconds(animation.Duration);
            }
        }

        yield return TransparentCo(1.5f);

        OnKilled?.Invoke(this);

        //TODO: 오브젝트 풀링은 현재는 안 사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }
    private IEnumerator SurviveCo(string animationName = "None")
    {
        if (animationName != "None")
        {
            spineController.SetAnimation(animationName, false);

            var animation = spineController.Skeleton.Data.FindAnimation(animationName);
            if (animation != null)
            {
                yield return new WaitForSeconds(animation.Duration);
            }
        }

        yield return TransparentCo(0.7f);

        OnSurvived?.Invoke(this);

        DefenceContext.Current.WaveSystem.AddSurvivedMonster(MonsterData.Name);

        //TODO: 오브젝트 풀링은 현재는 안사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }

    #endregion
}

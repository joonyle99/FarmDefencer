using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///
/// </summary>
public abstract class Monster : TargetableBehavior, IProduct
{
    [Header("──────── IProduct ────────")]
    [Space]

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

    [SerializeField] private string _monsterName;
    public string MonsterName => _monsterName;

    [Space]

    [SerializeField] private Transform _headPoint;
    public Transform HeadPoint
    {
        get
        {
            if (_headPoint == null)
            {
                _headPoint = this.transform;
            }

            return _headPoint;
        }
    }

    [Space]

    // default animation names
    [SpineAnimation]
    public string IdleAnimationName;
    [SpineAnimation]
    public string WalkAnimationName;
    [SpineAnimation]
    public string WalkDamagedAnimationName;
    [SpineAnimation]
    public string DissappearAnimationName;

    // events
    public event System.Action<int> OnDamaged;
    public event System.Action<Monster> OnKilled;
    public event System.Action<Monster> OnSurvived;

    private SortingGroup _sortingGroup;
    private int _defaultSortingOrder;

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

        var damageTextGo = Resources.Load<DamageText>("Prefabs/DamageText");
        var damageText = Instantiate(damageTextGo, Vector3.zero, Quaternion.identity);
        damageText.Init(damage.ToString(), type, HeadPoint);

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
        //Debug.Log("kill");

        IsDead = true;

        // TEMP: 이동 중지
        GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;

        SoundManager.Instance.PlaySfx($"SFX_D_{_monsterName}_dead");

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

        // TEMP: 이동 중지
        GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;

        StartCoroutine(SurviveCo(DissappearAnimationName));

        /*
        SetAnimation(DissappearAnimationName, false);

        //_spineAnimationState.Complete -= OnDissapearComplete_Survive;
        //_spineAnimationState.Complete += OnDissapearComplete_Survive;
        */
    }

    // color alpha
    private void InstantTransparent()
    {
        spineController.Skeleton.A = Mathf.Clamp01(0f);
    }
    private void InstantOpaque()
    {
        spineController.Skeleton.A = Mathf.Clamp01(1f);
    }
    private IEnumerator TransparentCo(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;

            spineController.Skeleton.A = Mathf.Clamp01(1f - t);

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

    private IEnumerator KillCo(string animationName)
    {
        //Debug.Log("kill routine");
        spineController.SetAnimation(animationName, false);

        var animation = spineController.Skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentCo(1.5f);

        OnKilled?.Invoke(this);

        //TODO: 오브젝트 풀링은 현재는 안사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }
    private IEnumerator SurviveCo(string animationName)
    {
        spineController.SetAnimation(animationName, false);

        var animation = spineController.Skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentCo(1.5f);

        OnSurvived?.Invoke(this);

        DefenceContext.Current.WaveSystem.AddSurvivedMonster(_monsterName);

        //TODO: 오브젝트 풀링은 현재는 안사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }
}

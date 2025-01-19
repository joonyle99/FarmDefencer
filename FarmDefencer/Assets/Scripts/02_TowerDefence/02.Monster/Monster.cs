using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Monster : TargetableBehavior, IProduct
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

    [SpineAnimation]
    public string IdleAnimationName;
    [SpineAnimation]
    public string WalkAnimationName;
    [SpineAnimation]
    public string WalkDamagedAnimationName;
    [SpineAnimation]
    public string DissappearAnimationName;

    private SkeletonAnimation _skeletonAnimation;
    private Spine.AnimationState _spineAnimationState;
    private Spine.Skeleton _skeleton;

    public event System.Action<int> OnDamaged;
    public event System.Action<Monster> OnKilled;
    public event System.Action<Monster> OnSurvived;

    protected override void Awake()
    {
        base.Awake();

        _skeletonAnimation = GetComponent<SkeletonAnimation>();

        _spineAnimationState = _skeletonAnimation.AnimationState;
        _skeleton = _skeletonAnimation.Skeleton;

        InstantTransparent();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        InstantOpaque();
    }
    private void OnDisable()
    {
        OnDamaged = null;
        OnKilled = null;
        OnSurvived = null;
    }
    protected override void Start()
    {
        base.Start();

        // Debug.Log("Monster Start()");
    }

    public override void TakeDamage(int damage)
    {
        if (IsDead == true)
        {
            return;
        }

        // 현재 실행 중인 애니메이션 확인
        var currentAnimation = _spineAnimationState.GetCurrent(0);

        if (currentAnimation != null
            && currentAnimation.Animation.Name != WalkDamagedAnimationName)
        {
            SetAnimation(WalkDamagedAnimationName, false);
            AddAnimation(WalkAnimationName, true);
        }

        HP -= damage;

        OnDamaged?.Invoke(damage);
    }
    public override void Kill()
    {
        IsDead = true;

        // TEMP: 이동 중지
        GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;

        StartCoroutine(KillRoutine(DissappearAnimationName));

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

        StartCoroutine(SurviveRoutine(DissappearAnimationName));

        /*
        SetAnimation(DissappearAnimationName, false);

        //_spineAnimationState.Complete -= OnDissapearComplete_Survive;
        //_spineAnimationState.Complete += OnDissapearComplete_Survive;
        */
    }

    // animation
    public void SetAnimation(string animationName, bool loop)
    {
        _spineAnimationState.SetAnimation(0, animationName, loop);
    }
    public void AddAnimation(string animationName, bool loop, float delay = 0f)
    {
        _spineAnimationState.AddAnimation(0, animationName, loop, delay);
    }

    // color alpha
    private void InstantTransparent()
    {
        _skeleton.A = Mathf.Clamp01(0f);
    }
    private void InstantOpaque()
    {
        _skeleton.A = Mathf.Clamp01(1f);
    }
    private IEnumerator TransparentRoutine(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;

            _skeleton.A = Mathf.Clamp01(1f - t);

            yield return null;

            eTime += Time.deltaTime;
        }

        InstantTransparent();
    }
    private IEnumerator OpaqueRoutine(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;

            _skeleton.A = Mathf.Clamp01(t);

            yield return null;

            eTime += Time.deltaTime;
        }

        InstantOpaque();
    }

    private IEnumerator KillRoutine(string animationName)
    {
        SetAnimation(animationName, false);

        var animation = _skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentRoutine(1.5f);

        OnKilled?.Invoke(this);

        OriginFactory.ReturnProduct(this);
    }
    private IEnumerator SurviveRoutine(string animationName)
    {
        SetAnimation(animationName, false);

        var animation = _skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentRoutine(1.5f);

        OnSurvived?.Invoke(this);

        DefenceContext.Current.WaveSystem.AddSurvivedMonster(_monsterName);

        OriginFactory.ReturnProduct(this);
    }
}

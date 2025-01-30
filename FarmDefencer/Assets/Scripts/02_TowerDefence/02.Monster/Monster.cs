using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

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

    [SerializeField] private SpineController _spineController;
    public SpineController SpineController => _spineController;

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

    protected override void Awake()
    {
        base.Awake();

        InstantTransparent();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        InstantOpaque();
    }
    private void OnDisable()
    {
        // stop all coroutines
        // 1. TransparentRoutine
        // 2. OpaqueRoutine
        // 3. KillRoutine
        // 4. SurviveRoutine
        // 5. StunRoutine
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

    public override void TakeDamage(int damage)
    {
        if (IsDead == true)
        {
            return;
        }

        HP -= damage;

        if (HP > 0)
        {
            // 현재 실행 중인 애니메이션 확인
            var currentAnimation = _spineController.SpineAnimationState.GetCurrent(0);
            if (currentAnimation != null
                && currentAnimation.Animation.Name != WalkDamagedAnimationName)
            {
                _spineController.SetAnimation(WalkDamagedAnimationName, false);
                _spineController.AddAnimation(WalkAnimationName, true);
            }

            //StartCoroutine(StunRoutine(StunDuration));
        }

        OnDamaged?.Invoke(damage);
    }
    public override void Kill()
    {
        //Debug.Log("kill");

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

    // color alpha
    private void InstantTransparent()
    {
        _spineController.Skeleton.A = Mathf.Clamp01(0f);
    }
    private void InstantOpaque()
    {
        _spineController.Skeleton.A = Mathf.Clamp01(1f);
    }
    private IEnumerator TransparentRoutine(float duration)
    {
        var eTime = 0f;

        while (eTime < duration)
        {
            var t = eTime / duration;

            _spineController.Skeleton.A = Mathf.Clamp01(1f - t);

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

            _spineController.Skeleton.A = Mathf.Clamp01(t);

            yield return null;

            eTime += Time.deltaTime;
        }

        InstantOpaque();
    }

    private IEnumerator KillRoutine(string animationName)
    {
        //Debug.Log("kill routine");
        _spineController.SetAnimation(animationName, false);

        var animation = _spineController.Skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentRoutine(1.5f);

        OnKilled?.Invoke(this);

        //TODO: 오브젝트 풀링은 현재는 안사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }
    private IEnumerator SurviveRoutine(string animationName)
    {
        _spineController.SetAnimation(animationName, false);

        var animation = _spineController.Skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            yield return new WaitForSeconds(animation.Duration);
        }

        yield return TransparentRoutine(1.5f);

        OnSurvived?.Invoke(this);

        DefenceContext.Current.WaveSystem.AddSurvivedMonster(_monsterName);

        //TODO: 오브젝트 풀링은 현재는 안사용하고 있기 때문에 주석 처리
        //OriginFactory.ReturnProduct(this);
        Destroy(this.gameObject);
    }
}

using Spine.Unity;
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

    // TODO: animation을 관리하는 클래스를 따로 만들기

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

    protected override void Awake()
    {
        base.Awake();

        _skeletonAnimation = GetComponent<SkeletonAnimation>();

        _spineAnimationState = _skeletonAnimation.AnimationState;
        _skeleton = _skeletonAnimation.Skeleton;
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        // Debug.Log("Monster OnEnable()");

        HP = StartHp;
    }
    protected override void Start()
    {
        base.Start();

        // Debug.Log("Monster Start()");
    }

    public override void TakeDamage(float damage)
    {
        HP -= (int)damage;
    }
    public override void Kill()
    {
        OriginFactory.ReturnProduct(this);
    }

    public void Survive()
    {
        // Debug.Log($"몬스터가 생존했습니다.");
        OriginFactory.ReturnProduct(this);
    }

    public void SetAnimation(string animationName, bool loop)
    {
        _spineAnimationState.SetAnimation(0, animationName, loop);
    }
}

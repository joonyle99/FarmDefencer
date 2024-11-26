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

    //[Header("──────── Monster ────────")]
    //[Space]

    protected override void OnEnable()
    {
        base.OnEnable();

        // Debug.Log("Monster OnEnable()");

        HP = StartHp;
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
}

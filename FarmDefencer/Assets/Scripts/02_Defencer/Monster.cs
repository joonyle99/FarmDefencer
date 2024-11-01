using UnityEngine;

public abstract class Monster : Targetable, IProduct
{
    // [Header("──────── IProduct ────────")]
    // [Space]

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

    // [Header("──────── Monster ────────")]
    // [Space]

    [SerializeField] private Targetter _targetter;

    public override void Hurt() { }
    public override void Die()
    {
        OriginFactory.ReturnObject(this);
    }

    // 만약 범위 안에 타워가 들어온다면 이동을 멈춰서 공격한다
    // 가장 가까운 타워를 찾아서 공격한다
    // 타겟이 사라졌다면 이동을 재개한다
    public void Attack()
    {
        // _towerDetector.Targets
    }
}

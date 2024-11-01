using UnityEngine;

public abstract class Monster : Targetable, IProduct
{
    // [Header("���������������� IProduct ����������������")]
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

    // [Header("���������������� Monster ����������������")]
    // [Space]

    [SerializeField] private Targetter _targetter;

    public override void Hurt() { }
    public override void Die()
    {
        OriginFactory.ReturnObject(this);
    }

    // ���� ���� �ȿ� Ÿ���� ���´ٸ� �̵��� ���缭 �����Ѵ�
    // ���� ����� Ÿ���� ã�Ƽ� �����Ѵ�
    // Ÿ���� ������ٸ� �̵��� �簳�Ѵ�
    public void Attack()
    {
        // _towerDetector.Targets
    }
}

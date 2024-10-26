using UnityEngine;

public abstract class Monster : Target, IProduct
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

    public override void Hurt() { }
    public override void Die()
    {
        OriginFactory.ReturnObject(this);
    }
}

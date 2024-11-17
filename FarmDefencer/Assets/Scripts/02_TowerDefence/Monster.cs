using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Monster : TargetableBehavior, IProduct
{
    [Header("���������������� IProduct ����������������")]
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

    [Header("���������������� Monster ����������������")]
    [Space]

    private int _hp = 50;
    public int HP
    {
        get => _hp;
        set
        {
            _hp = value;

            if (_hp < 0)
            {
                _hp = 0;
                Kill();
            }
        }
    }

    public override void TakeDamage(float damage)
    {
        HP -= (int)damage;
    }
    public override void Kill()
    {
        OriginFactory.ReturnProduct(this);
    }
}

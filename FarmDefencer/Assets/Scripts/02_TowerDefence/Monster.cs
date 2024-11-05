using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Monster : TargetableBehavior, IProduct
{
    // [Header("式式式式式式式式 IProduct 式式式式式式式式")]
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

    // [Header("式式式式式式式式 Monster 式式式式式式式式")]
    // [Space]

    public override void TakeDamage(float damage)
    {
        Debug.Log($"{this.gameObject.name} - take damage");
    }
    public override void Kill()
    {
        OriginFactory.ReturnProduct(this);
    }
}

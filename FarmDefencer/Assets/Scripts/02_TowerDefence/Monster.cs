using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Monster : TargetableBehavior, IProduct
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

    private void Start()
    {
        // TEMP

        /*
        var pathMovement = GetComponent<PathMovement>();
        var pathway = PathSupervisor.Instance.GetCurrentPathway();

        pathMovement.Initialize(pathway);
        */
    }

    public override void TakeDamage(float damage)
    {
        Debug.Log($"<color=orange>{this.gameObject.name}</color> take damage");
    }
    public override void Kill()
    {
        OriginFactory.ReturnProduct(this);
    }
}

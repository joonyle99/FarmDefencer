using UnityEngine;

/// <summary>
/// DamageableZone�� Damager�� DamageableBehavior�� �������ִ� ������ �Ѵ�.
/// </summary>
public abstract class DamageableZone : MonoBehaviour
{
    [SerializeField] protected DamageableBehavior demageableBehavior;
    protected Collider2D damageCollider;

    protected virtual void Awake()
    {
        damageCollider = GetComponent<Collider2D>();

        if (damageCollider == null)
        {
            throw new System.NullReferenceException($"You should add Collider2D component");
        }
    }
    /*
    public DamageableBehavior DamageableBehavior
    {
        get
        {
            if (_demageableBehavior == null)
            {
                Debug.LogWarning("_demageableBehavior is null. run LazyLoad");

                LazyLoad();

                if (_demageableBehavior == null)
                {
                    Debug.LogError("_demageableBehavior is still null after LazyLoad");
                }
            }

            return _demageableBehavior;
        }
    }

    private void LazyLoad()
    {
        _demageableBehavior = GetComponent<DamageableBehavior>();
    }
    */
}

using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class DamageableBehavior : MonoBehaviour
{
    // [Header("式式式式式式式式 DamageableBehavior 式式式式式式式式")]
    // [Space]

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageZone>();

        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }
    }

    public abstract void TakeDamage(float damage);
    public abstract void Kill();
}

using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class DamageableBehavior : MonoBehaviour
{
    // [Header("式式式式式式式式 TargetableBehavior 式式式式式式式式")]
    // [Space]

    public abstract void TakeDamage(float damage);
    public abstract void Kill();
}

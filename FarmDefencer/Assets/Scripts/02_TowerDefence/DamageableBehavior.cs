using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class DamageableBehavior : MonoBehaviour
{
    // [Header("���������������� TargetableBehavior ����������������")]
    // [Space]

    public abstract void TakeDamage(float damage);
    public abstract void Kill();
}

using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class TargetableBehavior : MonoBehaviour
{
    // [Header("���������������� TargetableBehavior ����������������")]
    // [Space]

    public abstract void TakeDamage();
    public abstract void Kill();
}

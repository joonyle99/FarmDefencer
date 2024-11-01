using UnityEngine;

/// <summary>
/// 타워의 공격을 받을 수 있는 대상입니다
/// </summary>
public abstract class Targetable : MonoBehaviour
{
    // [Header("──────── Targetable ────────")]
    // [Space]

    public abstract void Hurt();
    public abstract void Die();
}

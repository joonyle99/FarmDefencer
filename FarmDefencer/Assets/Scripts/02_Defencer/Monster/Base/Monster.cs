using UnityEngine;

/// <summary>
/// 타워의 공격 대상이 되는 몬스터의 기본 동작을 정의합니다
/// </summary>
public abstract class Monster : Target
{
    // [Header("──────── Monster ────────")]

    public override void Hurt() { }
    public override void Die() { }
}

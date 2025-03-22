using UnityEngine;

/// <summary>
/// 기본 투사체 탄환
/// </summary>
public sealed class LinearProjectile : ProjectileBase
{
    //[Header("──────── Linear Projectile ────────")]
    //[Space]

    protected override void Move()
    {
        // 타겟 방향으로 선형 이동
        var diffVec = (currentTarget.TargetPoint.position - transform.position).normalized;
        var velocity = diffVec * speed;
        transform.position += velocity * Time.deltaTime;
    }
}

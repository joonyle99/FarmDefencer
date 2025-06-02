using UnityEngine;

/// <summary>
/// 직선 투사체를 구현
/// </summary>
public abstract class LinearProjectile : ProjectileBase
{
    //[Header("──────── Linear Projectile ────────")]
    //[Space]

    protected override void Move()
    {
        // 투사체는 몬스터를 따라가는데, 속도가 느린 경우 부자연스러울 수 있음
        transform.position = Vector2.Lerp(startPos, currentTarget.transform.position, linearT);
    }
    protected override void Rotate()
    {
        // do nothing
    }
}

using UnityEngine;

/// <summary>
/// 기본 투사체 탄환
/// </summary>
public class LinearProjectile : ProjectileBase
{
    //[Header("──────── Linear Projectile ────────")]
    //[Space]

    protected override void Move()
    {
        transform.position = Vector2.Lerp(startPos, currentTarget.transform.position, linearT);
    }
    protected override void Rotate()
    {
        // do nothing
    }
    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, ProjectileType.Normal);
    }
    protected override void DealEffect()
    {
        // do nothing
    }
}

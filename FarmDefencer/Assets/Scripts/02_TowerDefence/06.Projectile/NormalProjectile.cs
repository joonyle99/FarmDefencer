using UnityEngine;

/// <summary>
/// 일반적인 투사체 탄환
/// 타격 시 데미지를 줍니다
/// </summary>
public sealed class NormalProjectile : LinearProjectile
{
    //[Header("──────── Normal Projectile ────────")]
    //[Space]

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, DamageType.Normal);
    }
    protected override void DealEffect()
    {
        // do nothing
    }
}

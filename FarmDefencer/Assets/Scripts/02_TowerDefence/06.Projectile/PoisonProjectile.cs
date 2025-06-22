using UnityEngine;

/// <summary>
/// 마비독 투사체 탄환
/// 타격 시 슬로우 효과를 적용
/// </summary>
public sealed class PoisonProjectile : LinearProjectile
{
    //[Header("──────── Poison Projectile ────────")]
    //[Space]

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, DamageType.Poison);
    }
    protected override void DealEffect()
    {
        // 슬로우 효과 적용 (중복 적용 가능)
        var slowEffector = currentTarget.gameObject.AddComponent<SlowEffector>();
        slowEffector.Activate(currentTarget, slowRate, slowDuration);
    }
}

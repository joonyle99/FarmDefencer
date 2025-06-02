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
        damager.DealDamage(currentTarget, ProjectileType.Poison);
    }
    protected override void DealEffect()
    {
        // REF
        //damager.DealEffect(currentTarget, _slowRate, _duration, ProjectileType.Poison);

        var slowEffector = currentTarget.GetComponent<SlowEffector>();
        if (slowEffector != null)
        {
            // 이미 슬로우 효과가 적용되어 있다면 지속 시간 초기화
            slowEffector.ReActivate();
        }
        else
        {
            // 슬로우 효과가 없다면 새로 생성하여 적용
            slowEffector = currentTarget.gameObject.AddComponent<SlowEffector>();
            slowEffector.Activate(currentTarget, slowRate, slowDuration);
        }
    }
}

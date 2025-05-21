using UnityEngine;

/// <summary>
/// 마비독 / 슬로우 투사체
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
        // 참고
        //damager.DealEffect(currentTarget, _slowRate, _duration, ProjectileType.Poison);

        var slowEffector = currentTarget.GetComponent<SlowEffector>();
        if (slowEffector != null)
        {
            slowEffector.ReActivate();
        }
        else
        {
            currentTarget.gameObject.AddComponent<SlowEffector>().Activate(currentTarget, slowRate, slowDuration);
        }
    }
}

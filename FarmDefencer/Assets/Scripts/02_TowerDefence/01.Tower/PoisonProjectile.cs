using UnityEngine;

/// <summary>
/// 마비독 / 슬로우 투사체
/// </summary>
public sealed class PoisonProjectile : LinearProjectile
{
    [Header("──────── Poison Projectile ────────")]
    [Space]

    [SerializeField] private float _slowDuration;
    [SerializeField] private float _slowPower;

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, ProjectileType.Poison);
    }
    protected override void DealEffect()
    {
        damager.DealEffect(currentTarget, ProjectileType.Poison);
    }
}

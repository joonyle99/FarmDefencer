using UnityEngine;

/// <summary>
/// 마비독 / 슬로우 투사체
/// </summary>
public sealed class PoisonProjectile : LinearProjectile
{
    [Header("──────── Poison Projectile ────────")]
    [Space]

    [Tooltip("값이 2인 경우 -> 속도를 0.5배로 줄임")]
    [SerializeField] private float _slowRate;
    [SerializeField] private float _duration;

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, ProjectileType.Poison);
    }
    protected override void DealEffect()
    {
        //damager.DealEffect(currentTarget, _slowRate, _duration, ProjectileType.Poison);
        var slowEffector = currentTarget.GetComponent<SlowEffector>();
        if (slowEffector != null)
        {
            slowEffector.ReActivate();
        }
        else
        {
            currentTarget.gameObject.AddComponent<SlowEffector>().Activate(currentTarget, _slowRate, _duration);
        }
    }
}

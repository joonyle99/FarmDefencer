using UnityEngine;

/// <summary>
/// 일반적인 레이저 빔
/// 지속적인 데미지를 줍니다
/// </summary>
public sealed class LaserBeam : BeamBase
{
    //[Header("──────── Laser Beam ────────")]
    //[Space]

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, DamageType.Laser);
    }
    protected override void DealEffect()
    {
        // do nothing
    }
}

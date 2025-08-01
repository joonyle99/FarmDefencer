using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 일반적인 레이저 빔
/// 지속적인 데미지를 줍니다
/// </summary>
public sealed class LaserBeam : BeamBase
{
    //[Header("──────── Laser Beam ────────")]
    //[Space]

    private ColorEffect _colorEffect;
    private bool _isColorEffectApplied = false;

    protected override void DealDamage()
    {
        damager.DealDamage(target, DamageType.Laser);
    }
    protected override void DealEffect()
    {
        if (_isColorEffectApplied)
        {
            return;
        }

        // 컬러 이펙트 적용
        _colorEffect = new ColorEffect(ConstantConfig.PINK, 0f, true);
        target.SpineController.AddColorEffect(_colorEffect);

        _isColorEffectApplied = true;
    }

    protected override void OnEndFunc()
    {
        target.SpineController.RemoveColorEffect(_colorEffect);

        _isColorEffectApplied = false;
    }
}
using UnityEngine;

/// <summary>
/// 대상에 번개 효과를 적용하는 컴포넌트
/// </summary>
public sealed class LightningEffector : EffectorBase
{
    private float _tempDuration = 0.5f;

    protected override void OnActivate(params object[] args)
    {
        if (args.Length != 3) return;

        var caster = (DamageableBehavior)args[0];
        var maxChainCount = (int)args[1];
        var curChainCount = (int)args[2];

        // 최대 체인 횟수를 초과하면 효과 적용 종료
        if (curChainCount >= maxChainCount)
        {
            return;
        }

        // 컬러 이펙트 적용
        ColorEffect colorEffect = new ColorEffect(ConstantConfig.YELLOW, _tempDuration);
        _damagableBehavior.SpineController.AddColorEffect(colorEffect);
    }
    protected override void OnDeactivate()
    {

    }
    protected override void OnEffectUpdate()
    {

    }
}

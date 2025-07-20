using UnityEngine;

/// <summary>
/// 대상에 번개 효과를 적용하는 컴포넌트
/// </summary>
public sealed class LightningEffector : EffectorBase
{
    private float _duration = 0.5f;
    private float _curDuration = 0f;

    protected override void OnActivate(params object[] args)
    {
        if (args.Length != 0) return;

        // 컬러 이펙트 적용
        ColorEffect colorEffect = new ColorEffect(ConstantConfig.YELLOW, _duration);
        affectedTarget.SpineController.AddColorEffect(colorEffect);
    }
    protected override void OnDeactivate()
    {
        // do something
    }
    protected override void OnEffectUpdate()
    {
        _curDuration += Time.deltaTime;

        if (_curDuration >= _duration)
        {
            DeActivate();
        }
    }
}

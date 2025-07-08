using UnityEngine;

/// <summary>
/// 대상에 슬로우 효과를 적용하는 컴포넌트
/// </summary>
public sealed class SlowEffector : EffectorBase
{
    private float _slowRate = 1f;
    private float _duration = 0f;

    private float _curDuration = 0f;

    protected override void OnActivate(params object[] args)
    {
        if (args.Length != 2) return;

        _slowRate = (float)args[0];
        _duration = (float)args[1];

        _curDuration = 0f;

        // TODO: 슬로우 효과가 중복으로 적용되는 것에 대한 처리가 필요하다..
        // 단순히 더하고 빼는 방식이 아닌 곱하고 나누는 방식이라 원래 값으로 돌아가지 못하는 경우가 발생한다 (기준이 바뀌어 버린다)
        // slowRate가 모두 동일하면 상관 없겠지만..?
        _damagableBehavior.GridMovement.SpeedFactor *= _slowRate;

        ColorEffect colorEffect = new ColorEffect(ConstantConfig.GREEN, _duration);
        _damagableBehavior.SpineController.AddColorEffect(colorEffect);
    }
    protected override void OnDeactivate()
    {
        _damagableBehavior.GridMovement.SpeedFactor /= _slowRate;
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

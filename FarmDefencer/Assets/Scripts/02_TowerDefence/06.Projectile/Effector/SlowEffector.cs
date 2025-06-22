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

        _damagableBehavior.GridMovement.MoveSpeed *= _slowRate;
    }
    protected override void OnDeactivate()
    {
        _damagableBehavior.GridMovement.MoveSpeed /= _slowRate;
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

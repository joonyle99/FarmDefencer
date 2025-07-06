using UnityEngine;

/// <summary>
/// 대상에 화상 효과를 적용하는 컴포넌트
/// </summary>
public sealed class BurnEffector : EffectorBase
{
    private int _tickCount = 0;
    private float _tickInterval = 0f;
    private int _tickDamage = 0;

    private int _curTickCount = 0;
    private float _curTickInterval = 0f;

    protected override void OnActivate(params object[] args)
    {
        if (args.Length != 3) return;

        _tickCount = (int)args[0];
        _tickInterval = (float)args[1];
        _tickDamage = (int)args[2];

        _curTickCount = 0;
        _curTickInterval = _tickInterval; // 즉시 데미지 주기 위해 초기값 설정

        ColorEffect colorEffect = new ColorEffect(ConstantConfig.RED, _tickCount * _tickInterval);
        _damagableBehavior.SpineController.AddColorEffect(colorEffect);
    }
    protected override void OnDeactivate()
    {
        // do something
    }
    protected override void OnEffectUpdate()
    {
        if (_curTickCount < _tickCount)
        {
            _curTickInterval += Time.deltaTime;
            if (_curTickInterval >= _tickInterval)
            {
                _curTickCount++;
                _curTickInterval = 0f;
                _damagableBehavior.TakeDamage(_tickDamage, DamageType.Burn);
            }
        }
        else
        {
            DeActivate();
        }
    }
}

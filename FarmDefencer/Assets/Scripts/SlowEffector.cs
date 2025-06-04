using UnityEngine;

public class SlowEffector : MonoBehaviour
{
    private float _slowRate = 1f;
    private float _duration = 0f;
    private float _startDuration = 0f;
    private bool _isActive = false;

    private DamageableBehavior _damagableBehavior;

    public void Activate(DamageableBehavior damagableBehavior, float slowRate, float duration)
    {
        this._damagableBehavior = damagableBehavior;
        this._slowRate = slowRate;
        this._duration = duration;
        this._startDuration = duration;

        if (!_isActive)
        {
            damagableBehavior.SpineController.SetColor(ConstantConfig.GREEN_GHOST);
            damagableBehavior.GridMovement.MoveSpeed *= slowRate;
            _isActive = true;
        }
    }
    public void ReActivate()
    {
        if (_isActive)
        {
            _duration = _startDuration;
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        _duration -= Time.deltaTime;
        if (_duration <= 0f)
        {
            _damagableBehavior.SpineController.ResetColor();
            _damagableBehavior.GridMovement.MoveSpeed /= _slowRate;
            _isActive = false;
            Destroy(this);
        }
    }
}

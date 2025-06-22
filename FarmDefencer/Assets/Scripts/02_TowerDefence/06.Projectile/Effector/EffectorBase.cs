using UnityEngine;

public abstract class EffectorBase : MonoBehaviour
{
    protected DamageableBehavior _damagableBehavior;
    protected bool _isActive = false;

    protected abstract void OnActivate(params object[] args);
    protected abstract void OnDeactivate();
    protected abstract void OnEffectUpdate();

    public virtual void Activate(DamageableBehavior damagableBehavior, params object[] args)
    {
        if (_isActive == true)
            return;

        _damagableBehavior = damagableBehavior;
        _isActive = true;
        OnActivate(args);
    }
    public virtual void DeActivate()
    {
        if (_isActive == false)
            return;

        _isActive = false;
        OnDeactivate();
        Destroy(this);
    }
    private void Update()
    {
        if (_isActive == false)
            return;

        OnEffectUpdate();
    }
}

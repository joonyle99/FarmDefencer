using UnityEngine;

public abstract class EffectorBase : MonoBehaviour
{
    protected DamageableBehavior affectedTarget;
    protected bool isActive = false;

    protected abstract void OnActivate(params object[] args);
    protected abstract void OnDeactivate();
    protected abstract void OnEffectUpdate();

    public virtual void Activate(DamageableBehavior target, params object[] args)
    {
        if (isActive == true)
            return;

        affectedTarget = target;
        isActive = true;
        OnActivate(args);
    }
    public virtual void DeActivate()
    {
        if (isActive == false)
            return;

        isActive = false;
        OnDeactivate();
        Destroy(this);
    }
    private void Update()
    {
        if (isActive == false)
            return;

        OnEffectUpdate();
    }
}

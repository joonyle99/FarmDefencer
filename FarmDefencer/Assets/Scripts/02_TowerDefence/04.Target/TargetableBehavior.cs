using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class TargetableBehavior : DamageableBehavior
{
    [Header("──────── TargetableBehavior ────────")]
    [Space]

    [SerializeField] private Transform _targetPoint;
    public Transform TargetPoint
    {
        get
        {
            if (_targetPoint == null)
            {
                _targetPoint = this.transform;
            }

            return _targetPoint;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
}

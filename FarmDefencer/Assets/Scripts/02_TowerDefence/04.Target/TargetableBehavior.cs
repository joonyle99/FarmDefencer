using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class TargetableBehavior : DamageableBehavior
{
    [Header("──────── TargetableBehavior ────────")]
    [Space]

    [SerializeField] private Transform _targetPoint;
    public Transform TargetPoint => _targetPoint;

    protected override void OnEnable()
    {
        base.OnEnable();
    }
}

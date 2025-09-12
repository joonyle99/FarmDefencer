using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class TargetableBehavior : DamageableBehavior
{
    [Header("──────── TargetableBehavior ────────")]
    [Space]

    private Transform _targetPoint;
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

    private Transform _headPoint;
    public Transform HeadPoint
    {
        get
        {
            if (_headPoint == null)
            {
                _headPoint = this.transform;
            }

            return _headPoint;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        _targetPoint = transform.Find("Center");
        _headPoint = transform.Find("Head");
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
}

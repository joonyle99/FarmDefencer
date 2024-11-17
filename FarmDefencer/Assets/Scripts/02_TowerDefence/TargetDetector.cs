using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TargetDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayerMask;

    [Space]

    private const int BUCKET_CAPACITY = 100;

    [SerializeField] private List<TargetableBehavior> _targetsInRange = new List<TargetableBehavior>(BUCKET_CAPACITY);
    public List<TargetableBehavior> TargetsInRange => _targetsInRange;

    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OntExitTarge;
    public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    // LEARNING POINT
    //
    // Collider2D 컴포넌트
    // -> Unity Physics Engine의 물리 연산과 함께 지속적으로 충돌을 체크한다
    //
    // Physics2D.OverlapCircleAll
    // -> 물리 연산이 아닌, 충돌 체크만을 수행한다

    public TargetableBehavior GetFrontTarget()
    {
        if (_targetsInRange.Count == 0)
        {
            return null;
        }

        return _targetsInRange[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check layer
        if (((1 << collision.gameObject.layer) & _targetLayerMask.value) > 0)
        {
            // check targetable
            if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
            {
                if (_targetsInRange.Contains(targetable) == false)
                {
                    _targetsInRange.Add(targetable);
                    OnEnterTarget?.Invoke(targetable);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
        {
            if (TargetsInRange.Contains(targetable) == true)
            {
                _targetsInRange.Remove(targetable);
                OntExitTarge?.Invoke(targetable);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 
/// </summary>
public class TargetDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayerMask;

    private const int BUCKET_CAPACITY = 100;

    private HashSet<TargetableBehavior> _targetsInRange = new HashSet<TargetableBehavior>(BUCKET_CAPACITY);
    public HashSet<TargetableBehavior> TargetsInRange => _targetsInRange;

    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OntExitTarge;
    public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    public List<TargetableBehavior> DebugTargets = new List<TargetableBehavior>();

    public TargetableBehavior CalcNearestTarget()
    {
        TargetableBehavior nearestTarget = null;
        var nearestDist = float.MaxValue;

        foreach (var target in _targetsInRange)
        {
            if (nearestTarget == null)
            {
                nearestTarget = target;
                continue;
            }

            var targetDist = Vector3.SqrMagnitude(this.transform.position - target.transform.position);

            if (targetDist < nearestDist)
            {
                nearestTarget = target;
                nearestDist = targetDist;
            }
        }

        return nearestTarget;
    }
    private void UpdateDebugTargets()
    {
        // O(N)
        DebugTargets = new List<TargetableBehavior>(TargetsInRange);
    }

    // LEARNING POINT
    //
    // Collider2D ������Ʈ
    // -> Unity Physics Engine�� ���� ����� �Բ� ���������� �浹�� üũ�Ѵ�
    //
    // Physics2D.OverlapCircleAll
    // -> ���� ������ �ƴ�, �浹 üũ���� �����Ѵ�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check layer
        if (((1 << collision.gameObject.layer) & _targetLayerMask.value) > 0)
        {
            // check targetable
            if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
            {
                // O(1)
                if (TargetsInRange.Add(targetable))
                {
                    OnEnterTarget?.Invoke(targetable);
                    UpdateDebugTargets();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
        {
            // O(1)
            if (TargetsInRange.Remove(targetable))
            {
                OntExitTarge?.Invoke(targetable);
                UpdateDebugTargets();
            }
        }
    }
}

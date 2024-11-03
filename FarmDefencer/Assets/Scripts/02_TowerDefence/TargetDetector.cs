using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TargetDetector : MonoBehaviour
{
    private const int BUCKET_CAPACITY = 100;

    private HashSet<TargetableBehavior> _targetsInRange = new HashSet<TargetableBehavior>(BUCKET_CAPACITY);
    public HashSet<TargetableBehavior> TargetsInRange => _targetsInRange;

    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OntExitTarge;
    public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    public List<TargetableBehavior> DebugTargets = new List<TargetableBehavior>();

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
        var target = collision.GetComponent<TargetableBehavior>();
        if (target == null)
        {
            return;
        }

        // O(1)
        if (TargetsInRange.Add(target))
        {
            OnEnterTarget?.Invoke(target);
            UpdateDebugTargets();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<TargetableBehavior>();
        if (target == null)
        {
            return;
        }

        // O(1)
        if (TargetsInRange.Remove(target))
        {
            OntExitTarge?.Invoke(target);
            UpdateDebugTargets();
        }
    }
}

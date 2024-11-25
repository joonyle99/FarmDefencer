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

    public int range = 3;


    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OntExitTarge;
    // public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    // LEARNING POINT
    //
    // Collider2D ������Ʈ
    // -> Unity Physics Engine�� ���� ����� �Բ� ���������� �浹�� üũ�Ѵ�
    //
    // Physics2D.OverlapCircleAll
    // -> ���� ������ �ƴ�, �浹 üũ���� �����Ѵ�

    public TargetableBehavior GetFrontTarget()
    {
        if (_targetsInRange.Count == 0)
        {
            return null;
        }

        return _targetsInRange[0];
    }

    private void Start()
    {
        DetectTargets();
    }
    private void Update()
    {

    }

    private void DetectTargets()
    {
        // �� TargetDetector�� ���� ���� Ÿ���� �����Ѵ�
        var cellPoint = GridMap.Instance.WorldToCell(this.transform.position);

        // ���� Ÿ�Ͽ��� range ������ �ش��ϴ� Ÿ���� ã�´�
        for(int h = -range; h <= range; h++)
        {
            for(int w = -range; w <= range; w++)
            {
                // var targetCellPoint = cellPoint + new Vector3Int(w, h, 0);
                var targetcCell = GridMap.Instance.GetCell(w, h);
                targetcCell.transform.localScale *= 2f;

                // var targetWorldPoint = GridMap.Instance.GetCellCenterWorld(targetCellPoint);

                // �ش� Ÿ�Ͽ� �ִ� ��� Ÿ���� ã�´�
                // var targets = Physics2D.OverlapCircleAll(targetWorldPoint, GridMap.Instance.UnitCellSize, _targetLayerMask);

                /*
                foreach (var target in targets)
                {
                    if (target.TryGetComponent<TargetableBehavior>(out var targetable))
                    {
                        if (_targetsInRange.Contains(targetable) == false)
                        {
                            _targetsInRange.Add(targetable);
                            OnEnterTarget?.Invoke(targetable);
                        }
                    }
                }
                */
            }
        }

        // ������ Ÿ�Ͽ��� raycast ��� (overlap�̳�) ���� �� ĭ�� targetLayer�� üũ�Ѵ�

        // ���� ����ȴٸ� list�� �߰��Ѵ�
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

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TargetableDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayerMask;

    [Space]

    [SerializeField] private int _widthRange = 1;
    [SerializeField] private int _heightRange = 1;

    [Space]

    [SerializeField] private List<TargetableBehavior> _currentTargets = new List<TargetableBehavior>(BUCKET_CAPACITY);

    private const int BUCKET_CAPACITY = 100;

    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OnExitTarget;
    // public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    public TargetableBehavior GetFrontTarget()
    {
        if (_currentTargets.Count == 0)
        {
            return null;
        }

        return _currentTargets[0];
    }

    private void Start()
    {
        DebugPaintRange();
    }
    private void Update()
    {
        DetectTargets();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check layer
        if (((1 << collision.gameObject.layer) & _targetLayerMask.value) > 0)
        {
            // check targetable
            if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
            {
                if (_currentTargets.Contains(targetable) == false)
                {
                    _currentTargets.Add(targetable);
                    OnEnterTarget?.Invoke(targetable);
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<TargetableBehavior>(out var targetable))
        {
            if (_currentTargets.Contains(targetable) == true)
            {
                _currentTargets.Remove(targetable);
                OnExitTarget?.Invoke(targetable);
            }
        }
    }

    // 
    private void DetectTargets()
    {
        // get origin cell
        var thisCellPos = GridMap.Instance.WorldToCell(transform.position);

        // new targets
        var newDetectedTargets = new HashSet<TargetableBehavior>(BUCKET_CAPACITY);

        // calc range
        for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        {
            for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
            {
                var offset = new Vector3Int(widthOffset, heightOffset, 0);
                if (offset == Vector3Int.zero)
                {
                    continue;
                }

                var targetCellPos = thisCellPos + offset;

                var targetCell = GridMap.Instance.GetCell(targetCellPos.x, targetCellPos.y);
                if (targetCell == null)
                {
                    continue;
                }

                // overlap circle
                var targetColliders = Physics2D.OverlapCircleAll(targetCell.transform.position, GridMap.Instance.UnitCellSize / 2f, _targetLayerMask);

                // detected targets
                foreach (var targetCollider in targetColliders)
                {
                    if (targetCollider.TryGetComponent<TargetableBehavior>(out var target))
                    {
                        if (target.IsDead == true)
                        {
                            continue;
                        }

                        if (newDetectedTargets.Contains(target) == true)
                        {
                            continue;
                        }

                        // �ϴ� �־��
                        newDetectedTargets.Add(target);

                        // ���ο� Ÿ���� �߰ߵǾ��ٸ� �߰�
                        if (_currentTargets.Contains(target) == false)
                        {
                            _currentTargets.Add(target);
                            OnEnterTarget?.Invoke(target);
                        }
                    }
                }
            }
        }

        // delete targets
        var deletableTargets = new List<TargetableBehavior>();

        // for out of range
        foreach (var oldTarget in _currentTargets)
        {
            // ���� ����
            // 1. ���ο� Ÿ�ٵ鿡 ���Ե��� �ʾƾ� �Ѵ�. (�ֳ��ϸ� newTargets�� ���Ӱ� Ž���ƴٴ� ���� ��Ÿ���� �����̴�.)
            if (newDetectedTargets.Contains(oldTarget) == false)
            {
                deletableTargets.Add(oldTarget);
            }
        }

        foreach (var deletableTarget in deletableTargets)
        {
            _currentTargets.Remove(deletableTarget);
            OnExitTarget?.Invoke(deletableTarget);
        }
    }

    // debug
    private void DebugPaintRange()
    {
        var order = 0;

        // get targetDetector's cell
        var thisCellPos = GridMap.Instance.WorldToCell(transform.position);

        // calc rectangle range
        for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        {
            for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
            {
                var offset = new Vector3Int(widthOffset, heightOffset, 0);
                var targetCellPos = thisCellPos + offset;

                var targetCell = GridMap.Instance.GetCell(targetCellPos.x, targetCellPos.y);
                if (targetCell == null)
                {
                    continue;
                }

                // debug
                order++;
                targetCell.DebugChangeColor(Color.blue);
                // targetCell.textMeshPro.text = order.ToString();
                // JoonyleGameDevKit.Painter.DebugDrawPlus(targetCell.transform.position, Color.red, GridMap.Instance.UnitCellSize / 2f, 5f);
            }
        }
    }
    public void DebugEraseRange()
    {
        // get targetDetector's cell
        var thisCellPos = GridMap.Instance.WorldToCell(transform.position);

        // calc rectangle range
        for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        {
            for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
            {
                var offset = new Vector3Int(widthOffset, heightOffset, 0);
                var targetCellPos = thisCellPos + offset;

                var targetCell = GridMap.Instance.GetCell(targetCellPos.x, targetCellPos.y);
                if (targetCell == null)
                {
                    continue;
                }

                // debug
                targetCell.DebugResetColor();
                // targetCell.textMeshPro.text = order.ToString();
                // JoonyleGameDevKit.Painter.DebugDrawPlus(targetCell.transform.position, Color.red, GridMap.Instance.UnitCellSize / 2f, 5f);
            }
        }
    }
}

// LEARNING POINT
//
// Collider2D ������Ʈ
// -> Unity Physics Engine�� ���� ����� �Բ� ���������� �浹�� üũ�Ѵ�
//
// Physics2D.OverlapCircleAll
// -> ���� ������ �ƴ�, �浹 üũ���� �����Ѵ�
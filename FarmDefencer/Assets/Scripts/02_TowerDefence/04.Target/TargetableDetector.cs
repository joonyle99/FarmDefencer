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

                        // 일단 넣어놔
                        newDetectedTargets.Add(target);

                        // 새로운 타겟이 발견되었다면 추가
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
            // 삭제 조건
            // 1. 새로운 타겟들에 포함되지 않아야 한다. (왜냐하면 newTargets는 새롭게 탐지됐다는 것을 나타내기 때문이다.)
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
// Collider2D 컴포넌트
// -> Unity Physics Engine의 물리 연산과 함께 지속적으로 충돌을 체크한다
//
// Physics2D.OverlapCircleAll
// -> 물리 연산이 아닌, 충돌 체크만을 수행한다
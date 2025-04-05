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

    [SerializeField] private int _widthRange;
    [SerializeField] private int _heightRange;

    private List<TargetableBehavior> _currentTargets = new(BUCKET_CAPACITY);

    private const int BUCKET_CAPACITY = 100;

    private TargetableBehavior _currentTarget;

    public event Action<TargetableBehavior> OnEnterTarget;
    public event Action<TargetableBehavior> OnExitTarget;
    // public event Action<TargetableBehavior> OnAccquireTarget;       // when you get current target

    private HashSet<GridCell> _paintedCells = new();

    public TargetableBehavior GetFrontTarget()
    {
        if (_currentTargets.Count == 0)
        {
            return null;
        }

        return _currentTargets[0];
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
        var thisCellPos = DefenceContext.Current.GridMap.WorldToCell(transform.position);

        // new targets
        // 중복 타겟 방지를 위해 HashSet을 사용한다.
        var newDetectedTargets = new HashSet<TargetableBehavior>(BUCKET_CAPACITY);

        // calc range
        for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        {
            for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
            {
                var offset = new Vector3Int(widthOffset, heightOffset, 0);
                if (offset == Vector3Int.zero)
                {
                    // 현재 위치는 탐지 범위에 포함되지 않는다.
                    continue;
                }

                var targetCellPos = thisCellPos + offset;

                var targetCell = DefenceContext.Current.GridMap.GetCell(targetCellPos.x, targetCellPos.y);
                if (targetCell == null)
                {
                    // 유효하지 않는 셀이라면 탐지 범위에 포함되지 않는다. (e.g 맵 밖)
                    continue;
                }

                // overlap circle
                var radius = DefenceContext.Current.GridMap.UnitCellSize / 2f;
                var targetColliders = Physics2D.OverlapCircleAll(targetCell.worldPosition, radius, _targetLayerMask);

                // detected targets
                foreach (var targetCollider in targetColliders)
                {
                    if (targetCollider.TryGetComponent<TargetableBehavior>(out var target))
                    {
                        // 죽은 타겟은 탐지 범위에 포함되지 않는다.
                        if (target.IsDead == true)
                        {
                            continue;
                        }

                        // 이미 탐지된 타겟이라면 탐지 범위에 포함되지 않는다.
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
        // 현재 탐지된 타겟들 중에서 탐지 범위에 포함되지 않는 타겟들을 삭제한다.
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

    /// <summary>
    /// 탐지 범위를 시각화하는 함수
    /// </summary>
    /// <param name="color">시각화 색상</param>
    public void PaintRange(Color? color = null)
    {
        EraseRange();

        // var order = 0;

        // get targetDetector's cell
        var thisCellPos = DefenceContext.Current.GridMap.WorldToCell(transform.position);

        // calc rectangle range
        for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        {
            for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
            {
                var offset = new Vector3Int(widthOffset, heightOffset, 0);

                if (offset == Vector3Int.zero)
                {
                    // 현재 위치는 탐지 범위에 포함되지 않는다.
                    continue;
                }

                var targetCellPos = thisCellPos + offset;
                var targetCell = DefenceContext.Current.GridMap.GetCell(targetCellPos.x, targetCellPos.y);
                if (targetCell == null)
                {
                    // 유효하지 않는 셀이라면 탐지 범위에 포함되지 않는다. (e.g 맵 밖)
                    continue;
                }

                _paintedCells.Add(targetCell);
                targetCell.ChangeColor(color ?? Color.blue);

                // debug
                // order++;
                // targetCell.distanceCostText.text = order.ToString();
                // JoonyleGameDevKit.Painter.DebugDrawPlus(targetCell.transform.position, Color.red, DefenceContext.Current.GridMap.UnitCellSize / 2f, 5f);
            }
        }
    }

    /// <summary>
    /// 탐지 범위를 지우는 함수
    /// </summary>
    public void EraseRange()
    {
        foreach (var paintedCell in _paintedCells)
        {
            paintedCell.ResetColor();
        }

        _paintedCells.Clear();

        // // get targetDetector's cell
        // var thisCellPos = DefenceContext.Current.GridMap.WorldToCell(transform.position);

        // // calc rectangle range
        // for (int widthOffset = -_widthRange; widthOffset <= _widthRange; widthOffset++)
        // {
        //     for (int heightOffset = -_heightRange; heightOffset <= _heightRange; heightOffset++)
        //     {
        //         var offset = new Vector3Int(widthOffset, heightOffset, 0);
        //         var targetCellPos = thisCellPos + offset;

        //         var targetCell = DefenceContext.Current.GridMap.GetCell(targetCellPos.x, targetCellPos.y);
        //         if (targetCell == null)
        //         {
        //             // 유효하지 않는 셀이라면 탐지 범위에 포함되지 않는다. (e.g 맵 밖)
        //             continue;
        //         }

        //         // debug
        //         targetCell.ResetColor();
        //         // targetCell.distanceCostText.text = order.ToString();
        //         // JoonyleGameDevKit.Painter.DebugDrawPlus(targetCell.transform.position, Color.red, DefenceContext.Current.GridMap.UnitCellSize / 2f, 5f);
        //     }
        // }
    }
}

// LEARNING POINT
//
// Collider2D 컴포넌트
// -> Unity Physics Engine의 물리 연산과 함께 지속적으로 충돌을 체크한다
//
// Physics2D.OverlapCircleAll
// -> 물리 연산이 아닌, 충돌 체크만을 수행한다
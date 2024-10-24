using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타워의 사거리 내에 있는 타겟들을 감지하고 관리하는 컴포넌트입니다.
/// </summary>
/// <remarks>
/// - HashSet으로 고효율의 타겟 관리
/// - Trigger Collider2D를 사용하여 타겟 감지
/// </remarks>
public class TargetDetector : MonoBehaviour
{
    private HashSet<Target> _targets = new HashSet<Target>();
    public HashSet<Target> Targets => _targets;

    [SerializeField] private List<Target> _targetsDebug = new List<Target>();

    private void UpdateTargetsDebug()
    {
        // HashSet의 내용을 List에 복사
        // O(n) 시간 복잡도
        _targetsDebug = new List<Target>(Targets);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<Target>();
        if (target == null) return;

        // O(1) 시간 복잡도
        if (Targets.Add(target))
        {
            UpdateTargetsDebug();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<Target>();
        if (target == null) return;

        // O(1) 시간 복잡도
        if (Targets.Remove(target))
        {
            UpdateTargetsDebug();
        }
    }
}

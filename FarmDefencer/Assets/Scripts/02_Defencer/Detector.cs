using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사거리 내에 있는 타겟들을 감지하고 관리하는 컴포넌트입니다.
/// </summary>
public class Detector<T> : MonoBehaviour where T : Component
{
    private const int BUCKET_SIZE = 1000;

    private HashSet<T> _targets = new HashSet<T>(BUCKET_SIZE);
    public HashSet<T> Targets => _targets;

    [SerializeField] private List<T> _targetsDebug = new List<T>();

    private void UpdateTargetsDebug()
    {
        // HashSet의 내용을 List에 복사
        // O(n) 시간 복잡도
        _targetsDebug = new List<T>(Targets);
    }

    // Collider2D 컴포넌트
    // -> Unity Physics Engine 물리 연산과 함께 지속적으로 충돌을 체크한다

    // Physics2D.OverlapCircleAll
    // -> 물리 연산이 아닌, 충돌 체크만을 수행한다

    // TODO: Raycast 방식으로 타겟을 감지하도록 변경하기

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<T>();
        if (target == null) return;

        // O(1) 시간 복잡도
        if (Targets.Add(target))
        {
            UpdateTargetsDebug();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<T>();
        if (target == null) return;

        // O(1) 시간 복잡도
        if (Targets.Remove(target))
        {
            UpdateTargetsDebug();
        }
    }
}

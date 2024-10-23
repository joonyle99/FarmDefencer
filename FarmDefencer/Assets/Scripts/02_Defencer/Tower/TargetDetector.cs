using System.Collections.Generic;
using UnityEngine;

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

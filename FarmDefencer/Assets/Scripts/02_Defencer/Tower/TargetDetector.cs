using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    private HashSet<Target> _targets = new HashSet<Target>();
    public HashSet<Target> Targets => _targets;

    [SerializeField] private List<Target> _targetsDebug = new List<Target>();

    private void UpdateTargetsDebug()
    {
        // HashSet�� ������ List�� ����
        // O(n) �ð� ���⵵
        _targetsDebug = new List<Target>(Targets);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<Target>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Add(target))
        {
            UpdateTargetsDebug();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<Target>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Remove(target))
        {
            UpdateTargetsDebug();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ÿ� ���� �ִ� Ÿ�ٵ��� �����մϴ�
/// </summary>
public class Targetter : MonoBehaviour
{
    private const int BUCKET_SIZE = 1000;

    private HashSet<Targetable> _targets = new HashSet<Targetable>(BUCKET_SIZE);
    public HashSet<Targetable> Targets => _targets;

    public List<Targetable> DebugTargets = new List<Targetable>();

    private void UpdateTargetsDebug()
    {
        // HashSet�� ������ List�� ����
        // O(n) �ð� ���⵵
        DebugTargets = new List<Targetable>(Targets);
    }

    // Collider2D ������Ʈ
    // -> Unity Physics Engine ���� ����� �Բ� ���������� �浹�� üũ�Ѵ�

    // Physics2D.OverlapCircleAll
    // -> ���� ������ �ƴ�, �浹 üũ���� �����Ѵ�

    // TODO: Raycast ������� Ÿ���� �����ϵ��� �����ϱ�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<Targetable>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Add(target))
        {
            UpdateTargetsDebug();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<Targetable>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Remove(target))
        {
            UpdateTargetsDebug();
        }
    }
}

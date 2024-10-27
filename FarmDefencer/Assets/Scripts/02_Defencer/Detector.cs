using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ÿ� ���� �ִ� Ÿ�ٵ��� �����ϰ� �����ϴ� ������Ʈ�Դϴ�.
/// </summary>
public class Detector<T> : MonoBehaviour where T : Component
{
    private const int BUCKET_SIZE = 1000;

    private HashSet<T> _targets = new HashSet<T>(BUCKET_SIZE);
    public HashSet<T> Targets => _targets;

    [SerializeField] private List<T> _targetsDebug = new List<T>();

    private void UpdateTargetsDebug()
    {
        // HashSet�� ������ List�� ����
        // O(n) �ð� ���⵵
        _targetsDebug = new List<T>(Targets);
    }

    // Collider2D ������Ʈ
    // -> Unity Physics Engine ���� ����� �Բ� ���������� �浹�� üũ�Ѵ�

    // Physics2D.OverlapCircleAll
    // -> ���� ������ �ƴ�, �浹 üũ���� �����Ѵ�

    // TODO: Raycast ������� Ÿ���� �����ϵ��� �����ϱ�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<T>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Add(target))
        {
            UpdateTargetsDebug();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var target = collision.GetComponent<T>();
        if (target == null) return;

        // O(1) �ð� ���⵵
        if (Targets.Remove(target))
        {
            UpdateTargetsDebug();
        }
    }
}

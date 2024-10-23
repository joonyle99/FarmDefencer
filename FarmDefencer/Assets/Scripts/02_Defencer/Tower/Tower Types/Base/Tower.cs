using System.Linq;
using UnityEngine;

public interface IAttackable
{
    public void Attack();
}

public abstract class Tower : MonoBehaviour, IAttackable
{
    private Transform _firePoint;
    private TargetDetector _targetDetector;
    private ITargetingStrategy _targetingStrategy;

    public void SetTargetingStrategy(ITargetingStrategy targetingStrategy)
    {
        _targetingStrategy = targetingStrategy;
    }

    // ���� ������ �Ұ��ΰ�?
    public void Attack()
    {
        var allTargets = _targetDetector.Targets.ToArray<Target>();

        /*
        var selectedTargets = _targetingStrategy.SelectTargets(allTargets);

        if (selectedTargets == null)
        {
            Debug.Log("There are no selectedTargets");
            return;
        }
        */

        foreach (var target in allTargets)
        {
            target.Hit();
        }
    }
}

/// ����ؾ� �ϴ� �Ӽ���

// Ÿ���� ü��
// Ÿ���� ����
// Ÿ���� ���ݷ�
// Ÿ���� ���ݼӵ�
// Ÿ���� ���� ȿ�� (���ο�, �ߵ�, ����)
// Ÿ���� �����Ÿ� ����
// Ÿ���� Ÿ���� ���� ��
// Ÿ���� ���� ��� (����, ����, ����)
// Ÿ���� Ÿ���� ��� (���� ����� ��, ü���� ���� ��, Ȥ�� ���� ���� ��)

// Ÿ�� ��ġ ���
// Ÿ�� ��ġ ��Ÿ��

// Ÿ�� ���׷��̵� �ý���?
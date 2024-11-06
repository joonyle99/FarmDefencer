using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("���������������� Tower ����������������")]
    [Space]

    [SerializeField] private TargetDetector _targetDetector;

    [Space]

    [SerializeField] private Damager _damager;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private float _elapsedAttackTime = 0f;

    private void Update()
    {
        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            // '= 0f'�� ���� �ʰ� '-=' ������ �ϴ� ������ ������ �ð���ŭ ������ �ؾ��ϱ� ����
            _elapsedAttackTime -= _intervalAttackTime;

            Attack();
        }
    }

    public void Attack()
    {
        var nearestTarget = _targetDetector.CalcNearestTarget();

        if (nearestTarget == null)
        {
            return;
        }

        // �̷��� ������� Tick()�� �༭ ��������� �߻�ü�� ��ó�� �����ϱ�
        var diffVec = nearestTarget.transform.position - _firePoint.position;
        var dirVec = diffVec.normalized;
        Debug.DrawRay(_firePoint.position, diffVec, Color.red, 0.1f);
        float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        _damager.SetDamage(10f);
        _damager.HasDamaged(nearestTarget);
    }

    public override void TakeDamage(float damage)
    {
        Debug.Log($"<color=yellow>{this.gameObject.name}</color> take damage");
    }

    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}

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

// Ÿ�� ���׷��̵�
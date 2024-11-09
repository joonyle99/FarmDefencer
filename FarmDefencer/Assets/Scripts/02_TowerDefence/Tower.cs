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

    [SerializeField] private TowerHead _head;
    [SerializeField] private Projectile _projectile;

    [Space]

    [SerializeField] private float _intervalAttackTime = 0.5f;

    private TargetableBehavior _currentTarget;
    private Projectile _currentProjectile;
    private float _elapsedAttackTime = 0f;

    private void Update()
    {
        UpdateDirection();

        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            // '= 0f'�� ���� �ʰ� '-=' ������ �ϴ� ������ ������ �ð���ŭ ������ �ؾ��ϱ� ����
            _elapsedAttackTime -= _intervalAttackTime;

            UpdateTarget();

            if (_currentTarget != null)
            {
                UpdateDirection();

                Attack();
            }
        }
    }

    public void UpdateTarget()
    {
        _currentTarget = _targetDetector.CalcNearestTarget();
    }
    public void UpdateDirection()
    {
        if (_currentTarget == null)
        {
            return;
        }

        var dirVec = (_currentTarget.transform.position - _head.transform.position).normalized;
        var targetAngle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0f, 0f, targetAngle + _head.StartAngle);
        _head.transform.rotation = targetRotation;
    }

    public void Attack()
    {
        _currentProjectile = Instantiate(_projectile, _head.Muzzle.position, _head.Muzzle.rotation);

        if (_currentProjectile == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        _currentProjectile.SetDamage(10);
        _currentProjectile.SetTarget(_currentTarget);
        _currentProjectile.Shoot();
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
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
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
            // '= 0f'로 하지 않고 '-=' 연산을 하는 이유는 누적된 시간만큼 공격을 해야하기 때문
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

// 타워의 체력
// 타워의 방어력

// 타워의 공격력
// 타워의 공격속도
// 타워의 공격 효과 (슬로우, 중독, 기절)

// 타워의 사정거리 범위
// 타워의 타게팅 가능 수
// 타워의 공격 대상 (지상, 공중, 수중)
// 타워의 타게팅 방식 (가장 가까운 적, 체력이 적은 적, 혹은 가장 빠른 적)

// 타워 설치 비용
// 타워 설치 쿨타임

// 타워 업그레이드
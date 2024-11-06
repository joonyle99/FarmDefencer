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

    [SerializeField] private Damager _damager;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private float _elapsedAttackTime = 0f;

    private void Update()
    {
        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            // '= 0f'로 하지 않고 '-=' 연산을 하는 이유는 누적된 시간만큼 공격을 해야하기 때문
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

        // 이러한 방식으로 Tick()을 줘서 즉발이지만 발사체인 것처럼 개발하기
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
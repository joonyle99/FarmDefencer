using System.Linq;
using UnityEngine;

/// <summary>
/// 적을 감지하고 공격하는 타워의 기본 동작을 정의합니다
/// </summary>
public abstract class Tower : MonoBehaviour, IAttackable
{
    [Header("──────── Tower ────────")]

    [Space]

    [SerializeField] private TargetDetector _targetDetector;

    [Space]
    
    [SerializeField] private Bullet _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private float _elapsedAttackTime = 0f;

    protected virtual void Update()
    {
        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            // '= 0f'로 하지 않고 '-=' 연산을 하는 이유는 누적된 시간만큼 공격을 해야하기 때문
            _elapsedAttackTime -= _intervalAttackTime;

            Attack();
        }
    }

    // Basic Functions
    public virtual void Attack()
    {
        var allTargets = _targetDetector.Targets.ToArray();

        // + TargetingStrategy를 이용해 타겟을 선택하는 로직 추가

        foreach (var target in allTargets)
        {
            var diffVec = target.transform.position - _firePoint.position;
            var dirVec = diffVec.normalized;

            Debug.DrawRay(_firePoint.position, diffVec, Color.red, 0.1f);

            float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            var bullet = Instantiate(_bullet, _firePoint.position, rotation);

            bullet.SetDamage(10);
            bullet.SetSpeed(15f);
            bullet.SetTarget(target);   // should set target before shoot

            bullet.Shoot();
        }
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

// 타워 업그레이드 시스템?
using System.Linq;
using UnityEngine;

public abstract class Tower : MonoBehaviour, IAttackable
{
    [Header("──────── Tower ────────")]

    [SerializeField] private TargetDetector _targetDetector;

    [Space]
    
    [SerializeField] private Bullet _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private ITargetingStrategy _targetingStrategy;
    [SerializeField] private float _intervalAttackTime = 0.5f;
    [SerializeField] private float _elapsedAttackTime = 0f;

    protected virtual void Update()
    {
        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            _elapsedAttackTime -= _intervalAttackTime;

            Attack();
        }
    }

    // Basic Functions
    public virtual void Attack()
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

        // 1. Shooting
        // -> 타워 디펜스에 등장하는 투사체의 종류를 살펴보자
        // 
        // 2. Instant Attack..?

        foreach (var target in allTargets)
        {
            var vec = target.transform.position - _firePoint.position;
            Debug.DrawRay(_firePoint.position, vec, Color.red, 0.5f);

            var dir = vec.normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var bullet = Instantiate(_bullet, _firePoint.position, Quaternion.Euler(0, 0, angle));

            bullet.SetDamage(10);
            bullet.SetSpeed(15f);
            bullet.SetDir(dir);
            bullet.SetTarget(target);
            bullet.Shoot();
        }
    }

    // Targetting Strategy
    public void SetTargetingStrategy(ITargetingStrategy targetingStrategy)
    {
        _targetingStrategy = targetingStrategy;
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
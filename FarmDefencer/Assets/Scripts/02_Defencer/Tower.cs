using System.Linq;
using UnityEngine;

/// <summary>
/// 적을 감지하고 공격하는 타워의 기본 동작을 정의합니다
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("──────── Tower ────────")]
    [Space]

    [SerializeField] private Targetter _targetter;

    [Space]
    
    [SerializeField] private Projectile _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private float _elapsedAttackTime = 0f;

    protected Targetter Targetter => _targetter;
    protected Projectile Bullet => _bullet;
    protected Transform FirePoint => _firePoint;

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

    // Basic Functions
    protected void Attack()
    {
        var allTargets = Targetter.Targets.ToArray<Targetable>();

        // + TargetingStrategy를 이용해 타겟을 선택하는 로직 추가

        foreach (var target in allTargets)
        {
            var diffVec = target.transform.position - FirePoint.position;
            var dirVec = diffVec.normalized;

            Debug.DrawRay(FirePoint.position, diffVec, Color.red, 0.1f);

            float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            var bullet = Instantiate(Bullet, FirePoint.position, rotation);

            bullet.SetDamage(10);
            bullet.SetSpeed(30f);
            bullet.SetTarget(target);   // should set target before shoot

            bullet.Shoot();
        }

        Debug.Log("BasicTower Attack");
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
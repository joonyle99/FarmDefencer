using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
    [Space]

    [SerializeField] private TargetDetector _targetDetector;
    public TargetDetector TargetDetector => _targetDetector;

    //[Space]
    
    //[SerializeField] private Projectile _bullet;
    //[SerializeField] private Transform _firePoint;
    //[SerializeField] private float _intervalAttackTime = 0.5f;

    //private float _elapsedAttackTime = 0f;

    //protected Projectile Bullet => _bullet;
    //protected Transform FirePoint => _firePoint;

    private void Update()
    {
        //_elapsedAttackTime += Time.deltaTime;

        //if (_elapsedAttackTime >= _intervalAttackTime)
        //{
        //    // '= 0f'로 하지 않고 '-=' 연산을 하는 이유는 누적된 시간만큼 공격을 해야하기 때문
        //    _elapsedAttackTime -= _intervalAttackTime;

        //    Attack();
        //}
    }

    public void Attack()
    {
        //TargetableBehavior[] allTargets = Targetter.TargetsInRange.ToArray<TargetableBehavior>();

        //foreach (Targetable target in allTargets)
        //{
        //    var diffVec = target.transform.position - FirePoint.position;
        //    var dirVec = diffVec.normalized;

        //    Debug.DrawRay(FirePoint.position, diffVec, Color.red, 0.1f);

        //    float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        //    Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        //    var bullet = Instantiate(Bullet, FirePoint.position, rotation);

        //    bullet.SetDamage(10);
        //    bullet.SetSpeed(30f);
        //    bullet.SetTarget(target);   // should set target before shoot

        //    bullet.Shoot();
        //}
    }

    public override void TakeDamage()
    {
        throw new System.NotImplementedException();
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
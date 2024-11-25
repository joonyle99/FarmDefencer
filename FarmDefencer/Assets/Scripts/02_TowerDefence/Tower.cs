using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("──────── Tower ────────")]
    [Space]

    [SerializeField] private TargetDetector _detector;

    [Space]

    [Header("Build")]
    [SerializeField] private int _cost = 10;
    public int Cost => _cost;
    // private int levelIndex;

    [Space]

    [Header("Fire")]
    [SerializeField] private TowerHead _head;
    [SerializeField] private Projectile _projectile;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private TargetableBehavior _currentTarget;
    private float _elapsedAttackTime = 0f;

    private void Update()
    {
        // update target every frame
        UpdateTarget();

        if (_currentTarget != null && _currentTarget.gameObject.activeSelf == true)
        {
            _head.LookAt(_currentTarget.transform.position);
        }

        _elapsedAttackTime += Time.deltaTime;

        if (_elapsedAttackTime >= _intervalAttackTime)
        {
            _elapsedAttackTime = 0f;

            if (_currentTarget != null && _currentTarget.gameObject.activeSelf == true)
            {
                Attack();
            }
        }
    }

    // build
    public bool IsValidBuild(int gold)
    {
        if (gold >= _cost)
        {
            return true;
        }

        return false;
    }

    // fire
    private void UpdateTarget()
    {
        _currentTarget = _detector.GetFrontTarget();
    }
    private void Attack()
    {
        var projectile = Instantiate(_projectile, _head.Muzzle.position, _head.Muzzle.rotation);

        if (projectile == null)
        {
            Debug.LogWarning($"projectile is null");
            return;
        }

        projectile.SetTarget(_currentTarget);
        projectile.Shoot();
    }

    // upgrade
    public void UpgradeRateOfFire()
    {

    }
    public void UpgradeDamage()
    {

    }

    public override void TakeDamage(float damage)
    {
        HP -= (int)damage;
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
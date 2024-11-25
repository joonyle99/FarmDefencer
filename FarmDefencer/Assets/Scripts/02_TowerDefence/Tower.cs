using UnityEngine;

/// <summary>
/// 
/// </summary>
public sealed class Tower : TargetableBehavior
{
    [Header("���������������� Tower ����������������")]
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
using System.Linq;
using UnityEngine;

public abstract class Tower : MonoBehaviour, IAttackable
{
    [Header("���������������� Tower ����������������")]

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
        // -> Ÿ�� ���潺�� �����ϴ� ����ü�� ������ ���캸��
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

// Ÿ�� ���׷��̵� �ý���?
using System.Linq;
using UnityEngine;

/// <summary>
/// ���� �����ϰ� �����ϴ� Ÿ���� �⺻ ������ �����մϴ�
/// </summary>
public abstract class Tower : MonoBehaviour, IAttackable
{
    [Header("���������������� Tower ����������������")]

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
            // '= 0f'�� ���� �ʰ� '-=' ������ �ϴ� ������ ������ �ð���ŭ ������ �ؾ��ϱ� ����
            _elapsedAttackTime -= _intervalAttackTime;

            Attack();
        }
    }

    // Basic Functions
    public virtual void Attack()
    {
        var allTargets = _targetDetector.Targets.ToArray();

        // + TargetingStrategy�� �̿��� Ÿ���� �����ϴ� ���� �߰�

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
using System.Linq;
using UnityEngine;

/// <summary>
/// ���� �����ϰ� �����ϴ� Ÿ���� �⺻ ������ �����մϴ�
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("���������������� Tower ����������������")]
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
            // '= 0f'�� ���� �ʰ� '-=' ������ �ϴ� ������ ������ �ð���ŭ ������ �ؾ��ϱ� ����
            _elapsedAttackTime -= _intervalAttackTime;

            Attack();
        }
    }

    // Basic Functions
    protected void Attack()
    {
        var allTargets = Targetter.Targets.ToArray<Targetable>();

        // + TargetingStrategy�� �̿��� Ÿ���� �����ϴ� ���� �߰�

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
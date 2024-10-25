using UnityEngine;

/// <summary>
/// ���� �����ϰ� �����ϴ� Ÿ���� �⺻ ������ �����մϴ�
/// </summary>
public abstract class Tower : MonoBehaviour
{
    [Header("���������������� Tower ����������������")]
    [Space]

    [SerializeField] private TargetDetector _targetDetector;

    [Space]
    
    [SerializeField] private Bullet _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _intervalAttackTime = 0.5f;

    private float _elapsedAttackTime = 0f;

    protected TargetDetector TargetDetector => _targetDetector;
    protected Bullet Bullet => _bullet;
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
    protected abstract void Attack();
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
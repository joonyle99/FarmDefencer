using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("──────── Projectile ────────")]

    [Space]

    [SerializeField] private int _damage = 10;
    [SerializeField] private float _speed = 20f;

    private TargetableBehavior _currentTarget;
    private bool _isTriggered = false;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        // TODO: _currentTarget가 사라지거나 어떠한 이유로 닿을 수 없다면 자동으로 파괴되는 기능이 있어야 한다
    }
    private void FixedUpdate()
    {
        if (_isTriggered && _currentTarget != null)
        {
            var diffVec = _currentTarget.transform.position - transform.position;
            var velocity = diffVec.normalized * _speed;

            _rigidbody.linearVelocity = velocity;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<TargetableBehavior>();
        if (target != null && target == _currentTarget)
        {
            _currentTarget.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
    public void SetTarget(TargetableBehavior target)
    {
        _currentTarget = target;
    }

    public void Shoot()
    {
        if (_currentTarget == null)
        {
            Debug.LogWarning("There is no target");
            return;
        }

        _isTriggered = true;
    }
}

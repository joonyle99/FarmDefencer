using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("──────── Projectile ────────")]

    [Space]

    // Damager 클래스 사용하기

    [SerializeField] private int _damage = 5;
    [SerializeField] private float _speed = 5f;
    
    private TargetableBehavior _target;
    private bool _isTriggered = false;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (_isTriggered && _target != null)
        {
            var diffVec = _target.transform.position - transform.position;
            var velocity = diffVec.normalized * _speed;

            _rigidbody.linearVelocity = velocity;
            // _rigidbody.AddForce(force);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<TargetableBehavior>();
        if (target != null && target == _target)
        {
            target.TakeDamage();
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
        _target = target;
    }

    public void Shoot()
    {
        if (_target == null)
        {
            Debug.LogWarning("There is no target");
            return;
        }

        _isTriggered = true;
    }
}

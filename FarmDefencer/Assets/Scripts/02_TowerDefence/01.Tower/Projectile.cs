using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("式式式式式式式式 Projectile 式式式式式式式式")]
    [Space]

    [SerializeField] private Damager _damager;
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
        if (_currentTarget == null || _currentTarget.gameObject.activeSelf == false)
        {
            Destroy(gameObject);
            return;
        }
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

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
    public void SetTarget(TargetableBehavior target)
    {
        _currentTarget = target;
    }

    // damager
    public int GetDamage()
    {
        return _damager.GetDamage();
    }
    public void SetDamage(int damage)
    {
        _damager.SetDamage(damage);
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

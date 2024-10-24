using UnityEngine;

public abstract class Bullet : MonoBehaviour, IHittable
{
    [Header("式式式式式式式式 Bullet 式式式式式式式式")]

    [SerializeField] private int _damage = 5;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private Vector2 _dir = Vector2.zero;
    [SerializeField] private Target _target = null;

    private bool _isTriggered = false;

    protected virtual void Update()
    {
        if (_isTriggered)
        {
            if (_target != null)
            {
                // if (Vector2.Distance(transform.position, _target.transform.position) < 1f) return;
                transform.position = Vector2.MoveTowards(transform.position, _target.transform.position, _speed * Time.deltaTime);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        var target = collision.GetComponent<Target>();
        if (target != null)
        {
            if (target == _target)
            {
                Debug.Log("だ惚");
                Destroy(gameObject);
                target.Hurt();
            }
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {

    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
    public void SetDir(Vector2 dir)
    {
        _dir = dir;
    }
    public void SetTarget(Target target)
    {
        _target = target;
    }

    public void Shoot()
    {
        if (_target == null)
        {
            Debug.LogWarning("_target is null");
            return;
        }

        _isTriggered = true;
    }

    public virtual void Hit()
    {
        throw new System.NotImplementedException();
    }
}

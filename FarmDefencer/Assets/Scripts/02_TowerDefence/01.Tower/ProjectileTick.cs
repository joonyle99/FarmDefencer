using UnityEngine;

public class ProjectileTick : MonoBehaviour
{
    [Header("──────── ProjectileTick ────────")]
    [Space]

    [SerializeField] private Damager _damager;
    [SerializeField] private float _speed = 20f;

    private TargetableBehavior _currentTarget;
    private bool _isTriggered = false;

    private void Update()
    {
        if (_currentTarget == null || _currentTarget.gameObject.activeSelf == false)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(_currentTarget.TargetPoint.position, transform.position) < 0.1f)
        {
            _damager.HasDamaged(_currentTarget);
            Destroy(gameObject);
            return;
        }

        if (_isTriggered)
        {
            var diffVec = _currentTarget.TargetPoint.position - transform.position;
            var velocity = diffVec.normalized * _speed;
            
            transform.position += velocity * Time.deltaTime;
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

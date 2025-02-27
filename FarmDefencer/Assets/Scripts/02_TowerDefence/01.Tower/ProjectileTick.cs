using UnityEngine;

/// <summary>
/// 기본 투사체 탄환
/// </summary>
public class ProjectileTick : MonoBehaviour
{
    [Header("──────── ProjectileTick ────────")]
    [Space]

    [SerializeField] private Damager _damager;      // damager에 따라..
    [SerializeField] private float _speed = 20f;

    private TargetableBehavior _currentTarget;
    private bool _isTriggered = false;

    private void Update()
    {
        // 트리거가 되지 않으면 리턴
        if (_isTriggered == false)
        {
            return;
        }

        // 타겟이 없으면 파괴
        if (_currentTarget == null || _currentTarget.gameObject.activeSelf == false)
        {
            Destroy(gameObject);
            return;
        }

        // 타겟에 닿으면 데미지를 주고 파괴
        if (Vector3.Distance(_currentTarget.TargetPoint.position, transform.position) < 0.05f)
        {
            _damager.HasDamaged(_currentTarget);
            Destroy(gameObject);
            return;
        }

        // 타겟 방향으로 이동
        var diffVec = _currentTarget.TargetPoint.position - transform.position;
        var velocity = diffVec.normalized * _speed;
        transform.position += velocity * Time.deltaTime;
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

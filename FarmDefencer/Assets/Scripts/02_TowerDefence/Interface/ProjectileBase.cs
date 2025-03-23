using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [Header("──────── ProjectileBase ────────")]
    [Space]

    [SerializeField] protected Damager damager;
    [SerializeField] protected float speed = 20f;
    [SerializeField] protected float hitThreshold = 0.05f;

    protected TargetableBehavior currentTarget;

    private bool _isTriggered = false;

    protected virtual void Update()
    {
        if (_isTriggered)
        {
            // 타겟이 없으면 파괴
            if (currentTarget == null || currentTarget.gameObject.activeSelf == false)
            {
                Destroy(gameObject);
                return;
            }

            // 타겟에 닿으면 데미지를 주고 파괴
            var sqrMagnitude = (currentTarget.TargetPoint.position - transform.position).sqrMagnitude;
            if (sqrMagnitude <= hitThreshold)
            {
                DealDamage();
                Destroy(gameObject);
                return;
            }

            Move();
            //Rotate();
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    public void SetDamage(int damage)
    {
        damager.SetDamage(damage);
    }
    public void SetTarget(TargetableBehavior target)
    {
        currentTarget = target;
    }

    public float GetSpeed()
    {
        return speed;
    }
    public int GetDamage()
    {
        return damager.GetDamage();
    }
    public TargetableBehavior GetTarget()
    {
        return currentTarget;
    }

    protected abstract void Move();
    protected virtual void Rotate()
    {
        var direction = currentTarget.TargetPoint.position - transform.position;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }
    protected virtual void DealDamage()
    {
        damager.HasDamaged(currentTarget);
    }

    public void Trigger()
    {
        _isTriggered = true;
    }
}

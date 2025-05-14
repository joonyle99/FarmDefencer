using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [Header("──────── ProjectileBase ────────")]
    [Space]

    [SerializeField] protected Damager damager;
    [SerializeField] protected float moveDuration = 1.5f;
    //[SerializeField] protected float hitThreshold = 0.05f;

    protected TargetableBehavior currentTarget;

    protected Vector3 startPos;
    protected bool isTriggered = false;
    protected float elapsedTime = 0f;
    protected float linearT = 0f;

    //public ProjectileData projectileData;

    private void Start()
    {
        startPos = transform.position;
    }
    private void Update()
    {
        if (isTriggered)
        {
            // 타겟이 없으면 파괴
            if (currentTarget == null || currentTarget.gameObject.activeSelf == false)
            {
                Destroy(gameObject);
                return;
            }

            if (linearT >= 1f)
            {
                DealDamage();
                DealEffect();
                Destroy(gameObject);
                return;
            }

            linearT = Mathf.Clamp01(elapsedTime / moveDuration);

            Move();
            Rotate();

            elapsedTime += Time.deltaTime;
        }
    }

    public void SetDamage(int damage)
    {
        damager.SetDamage(damage);
    }
    public void SetTarget(TargetableBehavior target)
    {
        currentTarget = target;
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
    protected abstract void Rotate();
    protected abstract void DealDamage();
    protected abstract void DealEffect();

    public void Trigger()
    {
        isTriggered = true;
    }
}

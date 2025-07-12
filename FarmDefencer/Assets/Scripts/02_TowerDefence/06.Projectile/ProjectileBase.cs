using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [Header("──────── ProjectileBase ────────")]
    [Space]

    // 기본
    [SerializeField] protected Damager damager;
    [SerializeField] protected float moveDuration = 1.5f;

    // 효과 - 슬로우
    protected float slowRate;
    protected float slowDuration;

    protected Tower caster;
    protected TargetableBehavior target;

    protected bool isTriggered = false;

    protected Vector3 startPos; // 투사체의 시작 위치 (생성 위치)
    protected float linearT = 0f; // 투사체의 이동 비율 (0f ~ 1f)
    protected float elapsedTime = 0f; // 투사체가 생성된 이후 이동 시간

    private void Start()
    {
        startPos = transform.position;
    }
    private void Update()
    {
        if (isTriggered)
        {
            // 타겟이 없으면 파괴
            if (target == null || target.gameObject.activeSelf == false)
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
    public void SetSlow(float slowRate, float slowDuration)
    {
        this.slowRate = slowRate;
        this.slowDuration = slowDuration;
    }
    public void SetCaster(Tower caster)
    {
        this.caster = caster;
    }
    public void SetTarget(TargetableBehavior target)
    {
        this.target = target;
    }

    public int GetDamage()
    {
        return damager.GetDamage();
    }
    public TargetableBehavior GetTarget()
    {
        return target;
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

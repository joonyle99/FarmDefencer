using UnityEngine;

public abstract class BeamBase : MonoBehaviour
{
    [Header("──────── BeamBase ────────")]
    [Space]

    // 기본
    [SerializeField] protected Damager damager;
    [SerializeField] protected float stayDuration;
    [SerializeField] protected float dealInterval;

    // 효과 - 슬로우
    protected float slowRate;
    protected float slowDuration;

    protected TargetableBehavior currentTarget;
    protected Tower currentTower;

    protected bool isTriggered = false;

    protected float elapsedTime = 0f;
    protected float elapsedInterval = 0f;

    protected LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    private void Start()
    {
        // TODO: A to B 선 그리기 (쭈욱 뻗어나가는 연출이 있으면 좋을 것 같다)
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, currentTarget.transform.position);
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

            if (elapsedTime > stayDuration)
            {
                Destroy(gameObject);
                return;
            }

            // 공격 범위를 벗어날 경우 공격 중지
            if (currentTower.Detector.IsIncludeTarget(currentTarget) == false)
            {
                Destroy(gameObject);
                return;
            }

            if (elapsedInterval > dealInterval)
            {
                elapsedInterval = 0f;

                DealDamage();
                DealEffect();
            }

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentTarget.TargetPoint.position);

            elapsedTime += Time.deltaTime;
            elapsedInterval += Time.deltaTime;
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
    public void SetTarget(TargetableBehavior target)
    {
        currentTarget = target;
    }

    public void SetStayDuration(float duration)
    {
        stayDuration = duration;
    }
    public void SetDealInterval(float interval)
    {
        dealInterval = interval;
    }
    public void SetTower(Tower tower)
    {
        currentTower = tower;
    }

    public int GetDamage()
    {
        return damager.GetDamage();
    }
    public TargetableBehavior GetTarget()
    {
        return currentTarget;
    }

    protected abstract void DealDamage();
    protected abstract void DealEffect();

    public void Trigger()
    {
        isTriggered = true;
    }
}

using UnityEngine;

/// <summary>
/// 대상을 타격하면 번개가 연쇄적으로 인접한 적들에게 전달됩니다.
/// </summary>
public sealed class LightningProjectile : LinearProjectile
{
    [Header("──────── Lightning Projectile ────────")]
    [Space]

    private DamageableBehavior _caster;
    private int _maxChainCount = 0;

    protected override void DealDamage()
    {
        damager.DealDamage(currentTarget, DamageType.Lightning);
    }
    protected override void DealEffect()
    {
        if (_caster == null)
        {
            return;
        }

        // _caster에 line renderer를 추가한다
        var lineRenderer = _caster.gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = _maxChainCount;
        // lineRenderer.SetPosition(0, _caster.transform.position);
        // lineRenderer.SetPosition(1, currentTarget.transform.position);
        // lineRenderer.startWidth = 0.1f;
        // lineRenderer.endWidth = 0.1f;
        // lineRenderer.material = new Material(Shader.Find("Standard"));

        // 연쇄 번개 효과 적용 (중복 적용 가능)
        var lightningEffector = currentTarget.gameObject.AddComponent<LightningEffector>();
        lightningEffector.Activate(currentTarget, _caster, _maxChainCount, 0);
    }

    public DamageableBehavior GetCaster()
    {
        return _caster;
    }
    public void SetCaster(DamageableBehavior caster)
    {
        _caster = caster;
    }

    public int GetMaxChainCount()
    {
        return _maxChainCount;
    }
    public void SetMaxChainCount(int maxChainCount)
    {
        _maxChainCount = maxChainCount;
    }
}

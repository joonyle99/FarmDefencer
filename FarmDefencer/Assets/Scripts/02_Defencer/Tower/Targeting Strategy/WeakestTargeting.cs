using UnityEngine;

/// <summary>
/// 현재 체력이 가장 낮은 타겟을 우선적으로 선택하는 전략
/// </summary>
public sealed class WeakestTargeting : TargetingStrategy
{
    public override Target[] SelectTargets(Target[] targets)
    {
        throw new System.NotImplementedException();
    }
}
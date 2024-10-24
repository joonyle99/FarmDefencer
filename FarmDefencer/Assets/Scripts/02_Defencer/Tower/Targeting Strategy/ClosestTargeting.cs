using UnityEngine;

/// <summary>
/// 타워와 가장 가까운 거리에 있는 타겟을 우선적으로 선택하는 전략
/// </summary>
public sealed class ClosestTargeting : TargetingStrategy
{
    public override Target[] SelectTargets(Target[] targets)
    {
        throw new System.NotImplementedException();
    }
}
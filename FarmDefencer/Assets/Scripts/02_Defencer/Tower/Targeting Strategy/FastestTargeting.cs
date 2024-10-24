using UnityEngine;

/// <summary>
/// 이동 속도가 가장 빠른 타겟을 우선적으로 선택하는 전략
/// </summary>
public sealed class FastestTargeting : TargetingStrategy
{
    public override Target[] SelectTargets(Target[] targets)
    {
        throw new System.NotImplementedException();
    }
}
using System;

/// <summary>
/// 타워의 사거리 내 존재하는 여러 타겟들 중에서 공격할 대상을 선별합니다.
/// </summary>
public abstract class TargetingStrategy : ITargetingStrategy
{
    public abstract Target[] SelectTargets(Target[] targets);
}

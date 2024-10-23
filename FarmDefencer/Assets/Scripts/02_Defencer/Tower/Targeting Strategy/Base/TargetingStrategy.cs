using UnityEngine;

public interface ITargetingStrategy
{
    Target[] SelectTargets(Target[] targets);
}

public abstract class TargetingStrategy : ITargetingStrategy
{
    // 공통 변수

    // 공통 함수

    public abstract Target[] SelectTargets(Target[] targets);
}

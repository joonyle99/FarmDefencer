using System;

public abstract class TargetingStrategy : ITargetingStrategy
{
    public abstract Target[] SelectTargets(Target[] targets);
}

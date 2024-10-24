public interface IAttackable
{
    public void Attack();
}

public interface ITargetable
{
    public void Hurt();
}

public interface IHittable
{
    public void Hit();
}

public interface ITargetingStrategy
{
    /// <summary>
    /// N 마리의 targets 중에서 M 마리의 targets을 선택합니다. (N >= M)
    /// </summary>
    Target[] SelectTargets(Target[] targets);
}
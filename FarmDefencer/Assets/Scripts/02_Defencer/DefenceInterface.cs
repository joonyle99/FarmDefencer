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
    /// N ������ targets �߿��� M ������ targets�� �����մϴ�. (N >= M)
    /// </summary>
    Target[] SelectTargets(Target[] targets);
}
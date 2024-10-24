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
    Target[] SelectTargets(Target[] targets);
}
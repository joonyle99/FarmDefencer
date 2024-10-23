using UnityEngine;

public interface ITargetingStrategy
{
    Target[] SelectTargets(Target[] targets);
}

public abstract class TargetingStrategy : ITargetingStrategy
{
    // ���� ����

    // ���� �Լ�

    public abstract Target[] SelectTargets(Target[] targets);
}

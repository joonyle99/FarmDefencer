using System;

/// <summary>
/// Ÿ���� ��Ÿ� �� �����ϴ� ���� Ÿ�ٵ� �߿��� ������ ����� �����մϴ�.
/// </summary>
public abstract class TargetingStrategy : ITargetingStrategy
{
    public abstract Target[] SelectTargets(Target[] targets);
}

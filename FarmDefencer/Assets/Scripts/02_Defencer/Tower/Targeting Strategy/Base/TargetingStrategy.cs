using UnityEngine;

/// <summary>
/// Ÿ���� ��Ÿ� �� �����ϴ� ���� Ÿ�ٵ� �߿��� ������ ����� �����մϴ�.
/// </summary>
public abstract class TargetingStrategy : MonoBehaviour
{
    public abstract Target[] SelectTargets(Target[] targets);
}

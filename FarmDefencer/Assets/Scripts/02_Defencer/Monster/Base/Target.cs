using UnityEngine;

/// <summary>
/// Ÿ���� ������ ���� �� �ִ� ���.
/// ���� �Ӹ� �ƴ϶�, ???�� Ÿ���� ������ ���� �� �ֽ��ϴ�
/// </summary>
public abstract class Target : MonoBehaviour, ITargetable
{
    public abstract void Hurt();
}

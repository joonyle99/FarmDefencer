using UnityEngine;

/// <summary>
/// Ÿ���� ������ ���� �� �ִ� ���.
/// ���� �Ӹ� �ƴ϶�, ???�� Ÿ���� ������ ���� �� �ֽ��ϴ�
/// </summary>
public abstract class Target : MonoBehaviour
{
    // [Header("���������������� Target ����������������")]

    public abstract void Hurt();
    public abstract void Die();
}

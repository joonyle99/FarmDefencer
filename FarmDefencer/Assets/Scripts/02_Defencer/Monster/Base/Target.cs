using UnityEngine;

public interface ITargetable
{
    public void Hit();
}

public abstract class Target : MonoBehaviour, ITargetable
{
    // ���� ����

    // ���� �Լ�

    public abstract void Hit();
}

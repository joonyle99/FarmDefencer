using UnityEngine;

public abstract class Monster : Target
{
    // ���� ����

    // ���� �Լ�

    public override void Hit()
    {
        Debug.Log("Monster Hit");
    }
}

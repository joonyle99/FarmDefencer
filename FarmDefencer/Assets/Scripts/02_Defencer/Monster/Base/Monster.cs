using UnityEngine;

public abstract class Monster : Target
{
    // [Header("���������������� Monster ����������������")]

    public override void Hurt()
    {
        Debug.Log("Monster Hurt");
    }
}

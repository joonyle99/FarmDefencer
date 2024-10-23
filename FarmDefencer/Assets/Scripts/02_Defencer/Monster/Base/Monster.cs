using UnityEngine;

public abstract class Monster : Target
{
    // 공통 변수

    // 공통 함수

    public override void Hit()
    {
        Debug.Log("Monster Hit");
    }
}

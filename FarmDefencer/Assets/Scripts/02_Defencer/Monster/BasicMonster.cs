using UnityEngine;

public sealed class BasicMonster : Monster
{
    public override void Hit()
    {
        Debug.Log("BasicMonster Hit");
    }
}

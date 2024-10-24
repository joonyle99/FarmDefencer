using UnityEngine;

public sealed class BasicMonster : Monster
{
    public override void Hurt()
    {
        base.Hurt();

        Debug.Log("BasicMonster Hurt");
    }
}

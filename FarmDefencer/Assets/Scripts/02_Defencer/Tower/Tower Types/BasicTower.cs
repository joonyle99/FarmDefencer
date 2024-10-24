using UnityEngine;

public sealed class BasicTower : Tower
{
    public override void Attack()
    {
        base.Attack();

        Debug.Log("BasicTower Attack");
    }
}

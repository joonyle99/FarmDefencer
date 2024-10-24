using UnityEngine;

public sealed class BasicBullet : Bullet
{
    public override void Hit()
    {
        base.Hit();

        Debug.Log("BasicBullet Hit");
    }
}

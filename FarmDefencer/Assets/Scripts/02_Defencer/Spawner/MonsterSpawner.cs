using UnityEngine;

public sealed class MonsterSpawner : Spawner
{
    protected override void Spawn()
    {
        Monster monster = Factory.GetObject<Monster>();
        monster.SetOriginFactory(Factory);
        monster.transform.position = transform.position;
    }
}

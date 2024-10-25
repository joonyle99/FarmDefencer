using UnityEngine;

public sealed class MonsterSpawner : Spawner
{
    protected override void SpawnObject()
    {
        Monster monster = Factory.GetObject<Monster>();
        SpawnedObjects.Add(monster.gameObject);

        monster.transform.position = transform.position;
    }
}

using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("收收收收收收收收 Spawner 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;
    public Factory Factory => _factory;

    [SerializeField] private List<GameObject> _spawnedObjects;
    public List<GameObject> SpawnedObjects => _spawnedObjects;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnObject();
        }
    }

    protected abstract void SpawnObject();
}

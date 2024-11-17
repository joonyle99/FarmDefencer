using JoonyleGameDevKit;
using System.Collections;
using UnityEngine;

public class Wave : MonoBehaviour
{
    [Header("收收收收收收收收 Wave 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;
    public Factory Factory => _factory;

    [SerializeField] private RangeFloat _waitTimeRange;
    [SerializeField] private float _waitTime = 0f;

    private float _elapsedTime = 0f;

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    protected void Spawn()
    {
        var monster = _factory.GetProduct<Monster>();
        var pathMovement = monster.GetComponent<PathMovement>();
        pathMovement.Initialize(PathSupervisor.Instance.GetRandomPathway());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            if (_elapsedTime >= _waitTime)
            {
                _elapsedTime = 0f;
                _waitTime = _waitTimeRange.Random();

                Spawn();
            }
            _elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

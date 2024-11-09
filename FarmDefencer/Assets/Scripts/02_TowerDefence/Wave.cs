using JoonyleGameDevKit;
using System.Collections;
using UnityEngine;

public class Wave : MonoBehaviour
{
    //[Header("收收收收收收收收 Wave 收收收收收收收收")]
    //[Space]

    //[SerializeField] private Factory _factory;
    //public Factory Factory => _factory;

    [SerializeField] private Monster _monster;
    [SerializeField] private RangeFloat _spawnRange;

    [Space]

    private float _waitTime = 0f;
    private float _elapsedTime = 0f;

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    protected void Spawn()
    {
        Debug.Log("Monster Spawn");

        //Monster monster = Factory.GetProduct<Monster>();
        //monster.SetOriginFactory(Factory);
        //monster.transform.position = transform.position;

        var monster = Instantiate(_monster, Vector3.zero, Quaternion.identity);
        var pathMovement = monster.GetComponent<PathMovement>();
        var targetPathway = PathSupervisor.Instance.GetRandomPathway();
        pathMovement.Initialize(targetPathway);
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            if (_elapsedTime >= _waitTime)
            {
                _elapsedTime = 0f;
                _waitTime = _spawnRange.Random();

                Spawn();
            }
            _elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

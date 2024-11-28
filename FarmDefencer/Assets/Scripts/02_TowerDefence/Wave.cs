using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Wave : MonoBehaviour
{
    [Header("收收收收收收收收 Wave 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;
    public Factory Factory => _factory;

    [Space]

    [SerializeField] private RangeFloat _waitTimeRange;
    [SerializeField] private float _waitTime = 0f;

    [Space]

    public List<Monster> _waveMonsters = new List<Monster>();

    private int _totalSpawnCount = 0;
    private int _targetSpawnCount = 10;

    public int TotalSpawnCount => _totalSpawnCount;
    public int TargetSpawnCount => _targetSpawnCount;

    public event System.Action<int> OnTotalSpawnCountChanged;

    public bool CompleteSpawn => _totalSpawnCount >= _targetSpawnCount;
    public bool CompleteWave => _waveMonsters.Count <= 0;

    private bool _isTriggered = false;

    private float _elapsedTime = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isTriggered == false)
        {
            _isTriggered = true;
            StartCoroutine(ProcessRoutine());
        }
    }

    private IEnumerator ProcessRoutine()
    {
        yield return GridMap.Instance.FindPathRoutine();
        yield return SpawnMonsterRoutine();
    }

    protected void Spawn()
    {
        if (CompleteSpawn)
        {
            return;
        }

        var monster = _factory.GetProduct<Monster>();
        var gridMovement = monster.GetComponent<GridMovement>();

        gridMovement.Initialize();

        _totalSpawnCount++;
        OnTotalSpawnCountChanged?.Invoke(_totalSpawnCount);

        monster.OnKilled -= RemoveWaveMonster;
        monster.OnKilled += RemoveWaveMonster;

        monster.OnSurvived -= RemoveWaveMonster;
        monster.OnSurvived += RemoveWaveMonster;

        AddWaveMonster(monster);
    }
    private IEnumerator SpawnMonsterRoutine()
    {
        while (true)
        {
            if (CompleteSpawn == true && CompleteWave == true)
            {
                yield return new WaitForSeconds(1f);

                CompleteProcess();

                yield break;
            }

            if (CompleteSpawn == false)
            {
                if (_elapsedTime >= _waitTime)
                {
                    _elapsedTime = 0f;
                    _waitTime = _waitTimeRange.Random();

                    Spawn();
                }
            }

            yield return null;

            _elapsedTime += Time.deltaTime;
        }
    }

    private void CompleteProcess()
    {
        Debug.Log("Complete Process !!!");

        if (TowerDefenceManager.Instance.SurvivedMonsters.Count > 0)
        {
            // failure
            EndingUI.Instance.ShowFailure();
        }
        else
        {
            // success
            EndingUI.Instance.ShowSuccess();
        }
    }

    private void AddWaveMonster(Monster monster)
    {
        _waveMonsters.Add(monster);
    }
    private void RemoveWaveMonster(Monster monster)
    {
        _waveMonsters.Remove(monster);
    }
}

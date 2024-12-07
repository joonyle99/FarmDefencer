using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    [Header("收收收收收收收收 WaveSystem 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;              // wave system use factory for spawn spawnedMonster
    public Factory Factory => _factory;

    [Space]

    [SerializeField] private RangeFloat _waitTimeRange;
    [SerializeField] private float _waitTime = 0f;

    [Space]

    private List<Monster> _aliveMonsters = new List<Monster>();

    // target spawn count
    private int _targetSpawnCount = 10;
    public int TargetSpawnCount
    {
        get => _targetSpawnCount;
        set
        {
            _targetSpawnCount = value;
            OnTargetSpawnCountChanged?.Invoke(_targetSpawnCount);
        }
    }
    public event System.Action<int> OnTargetSpawnCountChanged;

    // total spawn count
    private int _totalSpawnCount = 0;
    public int TotalSpawnCount
    {
        get => _totalSpawnCount;
        set
        {
            _totalSpawnCount = value;
            OnTotalSpawnCountChanged?.Invoke(_totalSpawnCount);
        }
    }
    public event System.Action<int> OnTotalSpawnCountChanged;

    // condition property
    public bool CompleteSpawn => _totalSpawnCount >= TargetSpawnCount;      // all monsters are spawnedMonster
    public bool CompleteWave => _aliveMonsters.Count <= 0;                  // all monsters are killed or survived

    // 
    private bool _isTriggered = false;
    private float _elapsedTime = 0f;

    private void Update()
    {
        // CHEAT: trigger wave system
        if (Input.GetKeyDown(KeyCode.Space) && _isTriggered == false)
        {
            _isTriggered = true;
            StartCoroutine(WaveProcessRoutine());
        }
    }

    private IEnumerator WaveProcessRoutine()
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

        var spawnedMonster = _factory.GetProduct<Monster>();
        var movement = spawnedMonster.GetComponent<GridMovement>();

        movement.Initialize();

        spawnedMonster.OnKilled -= RemoveMonster;
        spawnedMonster.OnKilled += RemoveMonster;

        spawnedMonster.OnSurvived -= RemoveMonster;
        spawnedMonster.OnSurvived += RemoveMonster;

        AddMonster(spawnedMonster);
    }
    private IEnumerator SpawnMonsterRoutine()
    {
        while (true)
        {
            // ending
            if (CompleteSpawn == true && CompleteWave == true)
            {
                yield return new WaitForSeconds(1f);

                CompleteWaveProcess();

                yield break;
            }

            // wave process
            if (CompleteSpawn == false)
            {
                // wait time
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

    private void CompleteWaveProcess()
    {
        Debug.Log("Complete Wave Process !!!");

        if (TowerDefenceManager.Instance.SurvivedCount > 0)
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

    private void AddMonster(Monster monster)
    {
        _aliveMonsters.Add(monster);

        TotalSpawnCount++;
    }
    private void RemoveMonster(Monster monster)
    {
        _aliveMonsters.Remove(monster);
    }
}

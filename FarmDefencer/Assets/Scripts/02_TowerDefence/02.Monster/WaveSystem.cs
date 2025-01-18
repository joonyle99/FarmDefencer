using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    [Header("收收收收收收收收 Wave System 收收收收收收收收")]
    [Space]

    [SerializeField] private Factory _factory;              // wave system use factory for spawn spawnedMonster
    public Factory Factory => _factory;

    [Space]

    [SerializeField] private RangeFloat _waitTimeRange;
    [SerializeField] private float _waitTime = 0f;

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

    [Space]

    private List<Monster> _fieldMonsters = new List<Monster>();
    private List<string> _survivedMonsters = new List<string>();

    public int FieldCount => _fieldMonsters.Count;
    public int SurvivedCount => _survivedMonsters.Count;

    public bool CompleteSpawn => _totalSpawnCount >= TargetSpawnCount;      // all monsters are spawnedMonster
    public bool CompleteWave => _fieldMonsters.Count <= 0;                  // all monsters are killed or survived

    public event System.Action<int> OnTargetSpawnCountChanged;
    public event System.Action<int> OnTotalSpawnCountChanged;
    public event System.Action<int> OnSurvivedCountChanged;

    public event System.Action OnSuccess;
    public event System.Action OnFailure;

    private bool _isTriggered = false;
    private float _elapsedTime = 0f;

    private void Update()
    {
        // CHEAT: trigger wave
        if (Input.GetKeyDown(KeyCode.Space) && _isTriggered == false)
        {
            _isTriggered = true;
            StartCoroutine(WaveProcessRoutine());
        }

        // CHEAT: fast clock
        if (Input.GetMouseButton(0))
        {
            Time.timeScale = 3f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // wave process
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
    private IEnumerator SpawnRoutine()
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
    private IEnumerator WaveProcessRoutine()
    {
        GameStateManager.Instance.ChangeState(GameState.Wave);

        yield return DefenceContext.Current.GridMap.FindPathRoutine();
        yield return SpawnRoutine();
    }

    //
    private void CompleteWaveProcess()
    {
        GameStateManager.Instance.ChangeState(GameState.End);

        if (SurvivedCount == 0)
        {
            OnSuccess?.Invoke();
        }
        else
        {
            OnFailure?.Invoke();
        }
    }

    // alive monsters
    private void AddMonster(Monster monster)
    {
        _fieldMonsters.Add(monster);

        TotalSpawnCount++;
    }
    private void RemoveMonster(Monster monster)
    {
        _fieldMonsters.Remove(monster);
    }

    // survive monsters
    public void AddSurvivedMonster(string monsterName)
    {
        _survivedMonsters.Add(monsterName);
        OnSurvivedCountChanged?.Invoke(_survivedMonsters.Count);
    }
    public void RemoveSurvivedMonster(string monsterName)
    {
        _survivedMonsters.Remove(monsterName);
        OnSurvivedCountChanged?.Invoke(_survivedMonsters.Count);
    }
}

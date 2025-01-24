using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터의 웨이브를 관리하는 시스템
/// </summary>
public class WaveSystem : MonoBehaviour
{
    [Header("━━━━━━━━ Wave System ━━━━━━━━")]
    [Space]

    [SerializeField] private Factory _factory;              // wave system use factory for spawn spawnedMonster
    public Factory Factory => _factory;

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

    [Space]

    public StageData stageData; // TEMP: 현재 1-1 스테이지에 대한 데이터만 존재함

    private void Update()
    {
        // CHEAT: trigger wave
        if (Input.GetKeyDown(KeyCode.Space) && _isTriggered == false)
        {
            _isTriggered = true;
            StartCoroutine(WaveProcessRoutine());
        }

        // CHEAT: fast clock
        if (Input.GetKey(KeyCode.BackQuote))
        {
            Time.timeScale = 3f;
        }
        else if (Input.GetKeyUp(KeyCode.BackQuote))
        {
            Time.timeScale = 1f;
        }
    }

    // wave process
    protected void Spawn()
    {
        if (CompleteSpawn == true)
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
        int waveCount = 1;

        // 모든 웨이브를 순회한다
        foreach (var wave in stageData.Waves)
        {
            var waveMonster = wave.WaveMonster;
            var spawnCount = wave.SpawnCountRange.Random();

            TargetSpawnCount = spawnCount;

            Debug.Log($"Wave {waveCount++} Start");
            Debug.Log($"Spawn Monster: {waveMonster.GetType()}, Spawn Count: {spawnCount}");

            // 해당 웨이브 진입
            // e.g) 토끼(8~12) + 토끼(4~6)
            while (true)
            {
                // complete wave
                if (CompleteSpawn == true)
                {
                    CompleteWaveProcess();
                    break;
                }

                // wait time
                if (_elapsedTime >= _waitTime)
                {
                    _elapsedTime = 0f;
                    _waitTime = _waitTimeRange.Random();

                }

                yield return null;

                _elapsedTime += Time.deltaTime;
            }

            // 다음 웨이브 대기
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitUntil(() => CompleteWave == true);

        CompleteStageProcess();
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
        // Do Something
    }
    private void CompleteStageProcess()
    {
        GameStateManager.Instance.ChangeState(GameState.End);

        if (SurvivedCount > 0)
        {
            OnFailure?.Invoke();
        }
        else
        {
            OnSuccess?.Invoke();
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

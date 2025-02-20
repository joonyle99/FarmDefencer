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

    [SerializeField] private RangeFloat _waitSpawnTimeRange;
    private float _waitSpawnTime = 0f;

    [Space]

    [SerializeField] private float _waitWaveTime = 2f;

    // target spawn count
    private int _targetSpawnCount = 0;
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

    // current spawn count
    private int _currentSpawnCount = 0;
    public int CurrentSpawnCount => _currentSpawnCount;

    [Space]

    private List<Monster> _fieldMonsters = new List<Monster>();
    public List<Monster> FieldMonsters => _fieldMonsters;
    private List<string> _survivedMonsters = new List<string>();

    public int FieldCount => _fieldMonsters.Count;
    public int SurvivedCount => _survivedMonsters.Count;

    public bool CompleteStage => _fieldMonsters.Count <= 0;                  // all monsters are killed or survived

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
        if (Input.GetKey(KeyCode.RightBracket))
        {
            Time.timeScale = 3f;
        }
        else if (Input.GetKeyUp(KeyCode.RightBracket))
        {
            Time.timeScale = 1f;
        }

        // CHEAT: slow clock
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            Time.timeScale = 0.3f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftBracket))
        {
            Time.timeScale = 1f;
        }
    }

    // wave process
    protected void Spawn(Monster monster)
    {
        _currentSpawnCount++;

        //TODO: 오브젝트 풀링을 여러 종류의 몬스터를 생성할 수 있도록 수정
        //var spawnedMonster = _factory.GetProduct<Monster>();
        var spawnedMonster = Instantiate(monster, Vector3.zero, Quaternion.identity);
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
        #region NEW

        int waveCount = 1;

        // 모든 웨이브를 순회한다
        foreach (var wave in stageData.Waves)
        {
            var waveMonster = wave.WaveMonster;
            var spawnCount = wave.SpawnCountRange.Random();

            TargetSpawnCount = spawnCount;

            //Debug.Log($"Wave {waveCount++} Start");
            //Debug.Log($"Spawn Monster: {waveMonster.GetType()}, Spawn Count: {spawnCount}");

            _currentSpawnCount = 0;

            // 해당 웨이브 진입
            // e.g) 토끼(8~12) + 토끼(4~6)
            while (true)
            {
                // complete wave
                if (_currentSpawnCount >= TargetSpawnCount)
                {
                    CompleteWaveProcess();
                    break;
                }

                // wait time
                if (_elapsedTime >= _waitSpawnTime)
                {
                    _elapsedTime = 0f;
                    _waitSpawnTime = _waitSpawnTimeRange.Random();

                    Spawn(waveMonster);
                }

                yield return null;

                _elapsedTime += Time.deltaTime;
            }

            //Debug.Log("다음 웨이브 대기 중");

            // 다음 웨이브 대기
            yield return new WaitForSeconds(_waitWaveTime);
        }

        yield return new WaitUntil(() => CompleteStage == true);

        CompleteStageProcess();

        #endregion

        #region OLD

        //while (true)
        //{
        //    // ending
        //    if (CompleteSpawn == true && CompleteStage == true)
        //    {
        //        // wait a second
        //        yield return new WaitForSeconds(1f);

        //        CompleteStageProcess();

        //        yield break;
        //    }

        //    // wave process
        //    if (CompleteSpawn == false)
        //    {
        //        // wait time
        //        if (_elapsedTime >= _waitSpawnTime)
        //        {
        //            _elapsedTime = 0f;
        //            _waitSpawnTime = _waitSpawnTimeRange.Random();

        //            Spawn();
        //        }
        //    }

        //    yield return null;

        //    _elapsedTime += Time.deltaTime;
        //}

        #endregion
    }
    private IEnumerator WaveProcessRoutine()
    {
        GameStateManager.Instance.ChangeState(GameState.Wave);

        yield return DefenceContext.Current.GridMap.FindPathOnStart();
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
    public bool CalculateEachPaths()
    {
        foreach (var fieldMonster in _fieldMonsters)
        {
            if (fieldMonster.IsDead == true)
            {
                continue;
            }
            var movemnet = fieldMonster.GetComponent<GridMovement>();
            if (movemnet != null)
            {
                bool result = movemnet.CalcEachGridPath();
                if (result == false)
                {
                    return false;
                }
            }
        }

        return true;
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

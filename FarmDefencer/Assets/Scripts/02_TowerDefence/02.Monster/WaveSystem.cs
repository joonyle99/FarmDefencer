using System;
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

    #region Attribute

    [SerializeField] private Factory _factory;                  // use factory for spawn monster
    public Factory Factory => _factory;

    [Space]

    // map - stage - wave
    [SerializeField] private List<MapData> _mapData;
    private StageData _stageData;
    private int _maxWaveCount;
    private int _currentWave;

    [Space]

    [SerializeField] private RangeFloat _waitSpawnTimeRange;    // 각 몬스터 스폰 간의 대기 시간
    private float _waitSpawnTime = 0f;
    [SerializeField] private RangeFloat _waitWaveTimeRange;     // 각 웨이브 간의 대기 시간
    private float _waitWaveTime = 0f;
    private float _elapsedTime = 0f;

    [Space]

    // progress bar
    [SerializeField] private ProgressBar _progressBar;

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

    private List<Monster> _fieldMonsters = new List<Monster>();
    public List<Monster> FieldMonsters => _fieldMonsters;
    private List<string> _survivedMonsters = new List<string>();
    public IReadOnlyList<string> SurvivedMonsters => _survivedMonsters;

    public int FieldCount => _fieldMonsters.Count;
    public int SurvivedCount => _survivedMonsters.Count;
    public bool CompleteStage => _fieldMonsters.Count <= 0; // all monsters are killed or survived

    public event System.Action<int> OnTargetSpawnCountChanged;
    public event System.Action<int> OnTotalSpawnCountChanged;
    public event System.Action<int> OnSurvivedCountChanged;

    public event System.Action<EndingType> OnEnding;

    #endregion

    #region Functions

    private void Start()
    {
        GameStateManager.Instance.OnWaveState += InitStageData;
        GameStateManager.Instance.OnWaveState += InitProgressBar;
        GameStateManager.Instance.OnWaveState += StartWaveProcess;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance is null)
        {
            return;
        }
        
        GameStateManager.Instance.OnWaveState -= InitStageData;
        GameStateManager.Instance.OnWaveState -= InitProgressBar;
        GameStateManager.Instance.OnWaveState -= StartWaveProcess;
    }

    private void Update()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        // TODO: 남은 웨이브에 따라서...?
        // 근데 웨이브 단위가 아닌 몬스터 단위로 해야 자연스러울 거 같은데
        // 그러려면 나올 몬스터를 미리 다 정해놔야 하지 않을까..?
        var remainWaveCount = _maxWaveCount - _currentWave;
        if (remainWaveCount > 0)
        {
            _progressBar.UpdateProgressBar((float)remainWaveCount, (float)_maxWaveCount);
        }
        else
        {
            _progressBar.UpdateProgressBar(0f, (float)_maxWaveCount);

            GameStateManager.Instance.ChangeState(GameState.WaveAfter);
        }
    }

    private void InitStageData()
    {
        var mapIndex = MapManager.Instance.CurrentMapIndex;
        var stageIndex = MapManager.Instance.CurrentStageIndex;
        var stageDataList = _mapData[mapIndex - 1].StateDataList;
        _stageData = stageDataList[stageIndex - 1];
        _maxWaveCount = _stageData.Waves.Count;
        _currentWave = 0;
    }
    private void InitProgressBar()
    {
        _progressBar.Initialize();
        _progressBar.SetDangerousThreshold(0.5f);
    }

    // wave process
    protected void Spawn(Monster monster)
    {
        _currentSpawnCount++;

        //TODO: 오브젝트 풀링을 여러 종류의 몬스터를 생성할 수 있도록 수정
        //var spawnedMonster = _factory.GetProduct<Monster>();
        var spawnedMonster = Instantiate(monster, Vector3.zero, Quaternion.identity);
        spawnedMonster.Activate(); // 몬스터 활성화

        spawnedMonster.GridMovement.Initialize();

        spawnedMonster.OnKilled -= RemoveMonster;
        spawnedMonster.OnKilled += RemoveMonster;

        spawnedMonster.OnSurvived -= RemoveMonster;
        spawnedMonster.OnSurvived += RemoveMonster;

        AddMonster(spawnedMonster);
    }
    private IEnumerator WaveSpawnCo()
    {
        // 모든 웨이브를 순회한다
        // 각 웨이브는 동일한 몬스터를 여러 번 스폰한다
        foreach (var wave in _stageData.Waves)
        {
            // wave setting
            _currentWave++;
            _waitWaveTime = _waitWaveTimeRange.Random(); // 웨이브 대기 시간

            // spawn setting
            var monster = wave.Monster;
            var spawnCount = wave.SpawnCountRange.Random(); // 몬스터 스폰 횟수
            TargetSpawnCount = spawnCount;
            _currentSpawnCount = 0;

            // 해당 웨이브 진입하여 몬스터를 스폰한다
            // e.g) 토끼(8~12)
            while (true)
            {
                // complete wave
                if (_currentSpawnCount >= TargetSpawnCount)
                {
                    // 웨이브 종료
                    CompleteWaveProcess();
                    break;
                }

                // wait spawn time for each same monster
                if (_elapsedTime >= _waitSpawnTime)
                {
                    _elapsedTime = 0f;
                    _waitSpawnTime = _waitSpawnTimeRange.Random(); // 몬스터 스폰 대기 시간

                    Spawn(monster);
                }

                yield return null;

                _elapsedTime += Time.deltaTime;
            }

            // 다음 웨이브 대기
            yield return new WaitForSeconds(_waitWaveTime);
        }

        // 모든 웨이브가 종료되면 스테이지 종료 처리
        yield return new WaitUntil(() => CompleteStage == true);

        // 스테이지 종료
        CompleteStageProcess();
    }
    public void StartWaveProcess()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.Wave
            && GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        StartCoroutine(WaveProcessCo());
    }
    private IEnumerator WaveProcessCo()
    {
        yield return DefenceContext.Current.GridMap.FindPathOnStartCo();
        yield return WaveSpawnCo();
    }

    // complete process
    private void CompleteWaveProcess()
    {
        // Do Something
        Debug.Log($"<color=orange>Complete {_currentWave} Wave</color>");
    }
    private void CompleteStageProcess()
    {
        if (GameStateManager.Instance.CurrentState is not GameState.WaveAfter)
            return;

        // TODO: Fight 버튼, 타워 설치, 등 불가능하게 막기
        GameStateManager.Instance.ChangeState(GameState.DefenceEnd);

        // 설치한 타워 가격의 총합
        var totalCost = DefenceContext.Current.GridMap.CalculateAllOccupiedTowerCost();

        if (SurvivedCount > 0)
        {
            // 실패 시 설치한 타워 가격의 100%를 돌려줌
            ResourceManager.Instance.EarnGold(totalCost);

            OnEnding?.Invoke(EndingType.Failure);
        }
        else
        {
            // 성공 시 설치한 타워 가격의 50%를 돌려줌
            ResourceManager.Instance.EarnGold((int)(totalCost * 0.5f));

            OnEnding?.Invoke(EndingType.Success);
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

            bool result = fieldMonster.GridMovement.CalcEachGridPath();
            if (result == false)
            {
                DefenceContext.Current.GridMap.LoadPrevDistanceCost();
                return false;
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

    #endregion
}

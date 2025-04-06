using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class PenaltyGiver : MonoBehaviour, IFarmUpdatable
{
    private const float AnimationFadeOutBeginTime = 3.0f;
    private const float AnimationEndTime = 5.0f;
    private const float CropLockTime = 5.0f;
    
    [Serializable]
    public class MapPenalty
    {
        [SerializeField] private MapEntry mapEntry;
        [SerializeField] private SurvivedMonsterPenalty penalty;

        public MapEntry MapEntry => mapEntry;
        public SurvivedMonsterPenalty Penalty => penalty;
    }

    private class PenaltyAnimationData
    {
        public Monster Monster { get; set; }
        public float ElapsedTime { get; set; }
    }

    private class PenaltyLockData
    {
        public CropLock CropLock { get; set; }
        public float TimesRemaining { get; set; }
    }

    private Dictionary<string, GameObject> _monsterPrefabCache;

    [Tooltip("중복되는 MapEntry에 대한 데이터가 존재하는 경우 가장 앞의 값이 반영됨.")]
    [SerializeField] private MapPenalty[] mapPenalties;

    [SerializeField] private GameObject cropLockPrefab;
    
    public bool IsAnimationPlaying => _animationDatas.Count > 0;
    
    private Farm _farm;
    private List<PenaltyAnimationData> _animationDatas;
    private List<PenaltyLockData> _lockDatas;

    public void Init(Farm farm)
    {
        _farm = farm;
    }

    public void SpawnMonsters(int mapId, IReadOnlyList<string> monsters)
    {
        var penaltyContext = mapPenalties.FirstOrDefault(mapPenalty => mapPenalty.MapEntry.MapId == mapId)?.Penalty;
        if (penaltyContext is null)
        {
            Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 MapId {mapId}에 해당하는 페널티를 찾을 수 없습니다.");
            return;
        }

        foreach (var monster in monsters)
        {
            if (!_monsterPrefabCache.TryGetValue(monster, out var monsterPrefab))
            {
                monsterPrefab = Resources.Load($"Prefabs/Monster/{monster}") as GameObject;
                _monsterPrefabCache.Add(monster, monsterPrefab);
            }
            
            if (monsterPrefab is null)
            {
                Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 Monster {monster} 에 대한 프리팹을 가져오지 못하였습니다.");
                continue;
            }
            
            var penalty = penaltyContext.MonsterPenaltyDatas.FirstOrDefault(m => m.Monster.Equals(monster));
            if (penalty is null)
            {
                Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 MapId {mapId} Monster {monster} 에 대한 페널티 정보를 찾지 못했습니다.");
                continue;
            }
            
            if (!_farm.TryGetLockableCropPositionFromProbability(penalty.CropProbabilityDatas, out var cropPosition))
            {
                Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 MapId {mapId} Monster {monster} 에게 페널티를 줄 작물을 찾지 못하였습니다.");
                continue;
            }

            if (!_farm.TryLockCropAt(cropPosition))
            {
                Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 MapId {mapId} Monster {monster} (이)가 작물을 잠그는 데에 실패하였습니다.");
                continue;
            }
            
            var monsterObject = Instantiate(monsterPrefab, transform);
            var monsterComponent = monsterObject.GetComponent<Monster>();
            monsterObject.transform.position = new Vector3(cropPosition.x, cropPosition.y, 0.0f);
            monsterComponent.SpineController.SetAnimation("eating", true);
            
            var penaltyPlayingData = new PenaltyAnimationData
            {
                Monster = monsterComponent,
            };
            _animationDatas.Add(penaltyPlayingData);
        }
    }
    
    public void OnFarmUpdate(float deltaTime)
    {
        // 잠금 시간 동작은 FarmClock의 영향을 받음
        
        for (var i = _lockDatas.Count-1; i >= 0; --i)
        {
            var lockData = _lockDatas[i];

            lockData.TimesRemaining -= deltaTime;
            lockData.CropLock.UpdateTime(lockData.TimesRemaining);
            if (lockData.TimesRemaining < 0.0f)
            {
                // 오브젝트 삭제
                var position = lockData.CropLock.transform.position;
                Destroy(lockData.CropLock.gameObject);
                
                // 잠금 해제
                _farm.UnlockCropAt(position);
                
                _lockDatas.RemoveAt(i);
            }
        }
    }

    private void Awake()
    {
        _animationDatas = new();
        _lockDatas = new();
        _monsterPrefabCache = new();
    }

    private void Update()
    {
        // 애니메이션 재생은 FarmClock 정지 상태와 무관하므로 Update()에서 처리
        
        var deltaTime = Time.deltaTime;
        
        for (var i = _animationDatas.Count-1; i >= 0; --i)
        {
            var animationData = _animationDatas[i];
            animationData.ElapsedTime += deltaTime;
            if (animationData.ElapsedTime >= AnimationEndTime)
            {
                // 애니메이션 오브젝트 삭제
                var position = animationData.Monster.transform.position;
                Destroy(animationData.Monster.gameObject);
                _animationDatas.RemoveAt(i);
                
                // CropLock 오브젝트 스폰
                var cropLockObject = Instantiate(cropLockPrefab, transform);
                var cropLockComponent = cropLockObject.GetComponent<CropLock>();
                cropLockObject.transform.position = position;
                var lockData = new PenaltyLockData
                {
                    CropLock = cropLockComponent,
                    TimesRemaining = CropLockTime
                };
                cropLockComponent.UpdateTime(lockData.TimesRemaining); // 초기값 설정
                _lockDatas.Add(lockData);
            }
            else if (animationData.ElapsedTime >= AnimationFadeOutBeginTime)
            {
                var alpha = Mathf.Clamp((AnimationEndTime - animationData.ElapsedTime) / (AnimationEndTime - AnimationFadeOutBeginTime), 0.0f, 1.0f);
                animationData.Monster.SpineController.Skeleton.A = alpha;
            }
        }
    }
}

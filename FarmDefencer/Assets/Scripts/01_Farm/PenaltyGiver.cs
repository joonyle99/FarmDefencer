using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed partial class PenaltyGiver : MonoBehaviour, IFarmUpdatable, IFarmSerializable
{
    private const float AnimationFadeOutBeginTime = 4.0f;
    private const float AnimationEndTime = 5.0f;
    private const float CropLockTime = 60.0f;

    [Serializable]
    public class MapPenalty
    {
        [SerializeField] private MapEntry mapEntry;
        [SerializeField] private SurvivedMonsterPenalty penalty;

        public MapEntry MapEntry => mapEntry;
        public SurvivedMonsterPenalty Penalty => penalty;
    }
    
    private Dictionary<string, GameObject> _monsterPrefabCache;

    [Tooltip("중복되는 MapEntry에 대한 데이터가 존재하는 경우 가장 앞의 값이 반영됨.")] [SerializeField]
    private MapPenalty[] mapPenalties;

    [SerializeField] private GameObject cropLockPrefab;

    public bool IsAnimationPlaying => _survivedMonsters.Count > 0;

    private Farm _farm;
    private List<EatingMonster> _survivedMonsters;
    private List<CropLocker> _cropLockers;

    public JObject Serialize()
    {
        var array = new JArray();
        foreach (var cropLocker in _cropLockers)
        {
            var pos = cropLocker.LockPosition;

            var json = new JObject
            {
                ["X"] = pos.x,
                ["Y"] = pos.y,
                ["RemainingTime"] = cropLocker.RemainingTime
            };

            array.Add(json);
        }
        foreach (var monster in _survivedMonsters)
        {
            var pos = monster.EatingPosition;

            var json = new JObject
            {
                ["X"] = pos.x,
                ["Y"] = pos.y,
                ["RemainingTime"] = CropLockTime
            };
            array.Add(json);
        }

        return new JObject(new JProperty("CropLockers", array));
    }

    public void Deserialize(JObject json)
    {
        _survivedMonsters.ForEach(DestroyMonster);
        _survivedMonsters.Clear();

        _cropLockers.ForEach(DestroyLocker);
        _cropLockers.Clear();

        if (json["CropLockers"] is JArray array)
        {
            foreach (var jsonCropLocker in array)
            {
                if (jsonCropLocker.Type != JTokenType.Object)
                {
                    continue;
                }

                var jsonCropLockerObject = (JObject)jsonCropLocker;

                var x = jsonCropLockerObject["X"].Value<float?>() ?? 0.0f;
                var y = jsonCropLockerObject["Y"].Value<float?>() ?? 0.0f;

                if (!_farm.TryLockCropAt(new Vector2(x, y)))
                {
                    Debug.LogError($"PenaltiGiver.Deserialize()에서 {x}, {y} 에 해당하는 작물을 잠그지 못했습니다.");
                    continue;
                }
                
                var lockTime = jsonCropLockerObject["RemainingTime"].Value<float?>() ?? CropLockTime;
                SpawnCropLockerAt(new Vector2(x, y), lockTime);
            }
        }
    }

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
            var penalty = penaltyContext.MonsterPenaltyDatas.FirstOrDefault(m => m.Monster.Equals(monster));
            if (penalty is null)
            {
                Debug.LogError(
                    $"PenaltyGiver.SpawnMonsters()에서 MapId {mapId} Monster {monster} 에 대한 페널티 정보를 찾지 못했습니다.");
                continue;
            }

            if (!_farm.TryGetLockableCropPositionFromProbability(penalty.CropProbabilityDatas, out var cropPosition, out var productEntry))
            {
                Debug.LogError(
                    $"PenaltyGiver.SpawnMonsters()에서 MapId {mapId} Monster {monster} (이)가 파괴할 작물을 찾지 못하였습니다.");
                continue;
            }

            SpawnMonsterAt(monster, cropPosition, productEntry.ProductName);
        }
    }

    public void OnFarmUpdate(float deltaTime)
    {
        // 잠금 시간 동작은 FarmClock의 영향을 받음
        
        _cropLockers.ForEach(c => c.UpdateLock(deltaTime));
        foreach (var expiredLocker in _cropLockers.Where(c => c.IsDone))
        {
            DestroyLocker(expiredLocker);
        }

        _cropLockers.RemoveAll(c => c.IsDone);
    }

    private void Awake()
    {
        _survivedMonsters = new();
        _cropLockers = new();
        _monsterPrefabCache = new();
    }

    private void Update()
    {
        // 애니메이션 재생은 FarmClock 정지 상태와 무관하므로 Update()에서 처리

        var deltaTime = Time.deltaTime;

        _survivedMonsters.ForEach(m => m.UpdateAnimation(deltaTime));
        foreach (var expiredMonster in _survivedMonsters
                     .Where(m => m.IsDone))
        {
            // 잠금 스폰
            SpawnCropLockerAt(expiredMonster.EatingPosition, CropLockTime);
            
            // 몬스터 삭제
            DestroyMonster(expiredMonster);
        }

        _survivedMonsters.RemoveAll(m => m.IsDone);
    }

    private void DestroyLocker(CropLocker locker)
    {
        _farm.UnlockCropAt(locker.LockPosition);
        locker.DestroyLock();
    }

    private void DestroyMonster(EatingMonster monster) => monster.DestroyMonster();

    private void SpawnMonsterAt(string monsterName, Vector2 cropWorldPosition, string productName)
    {
        if (!_monsterPrefabCache.TryGetValue(monsterName, out var monsterPrefab))
        {
            monsterPrefab = Resources.Load($"Prefabs/Monster/{monsterName}") as GameObject;
            _monsterPrefabCache.Add(monsterName, monsterPrefab);
        }

        if (monsterPrefab is null)
        {
            Debug.LogError($"PenaltyGiver.SpawnMonsters()에서 Monster {monsterName} 에 대한 프리팹을 가져오지 못하였습니다.");
            return;
        }

        if (!_farm.TryLockCropAt(cropWorldPosition))
        {
            Debug.LogError(
                $"PenaltyGiver.SpawnMonsterAt()에서 Monster {monsterName} (이)가 작물을 잠그는 데에 실패하였습니다.");
            return;
        }

        var monsterObject = Instantiate(monsterPrefab, transform);
        var monsterComponent = monsterObject.GetComponent<Monster>();
        monsterObject.transform.position = new Vector3(cropWorldPosition.x, cropWorldPosition.y, 0.0f);
        monsterComponent.SpineController.SetAnimation("eating", false);
        monsterComponent.SpineController.AddAnimation("eating", false);
        var eatingAnimationLength =
            monsterComponent.SpineController.Skeleton.Data.FindAnimation("eating").Duration;
        monsterComponent.SpineController.SkeletonAnimation.timeScale =
            2 * eatingAnimationLength / AnimationFadeOutBeginTime;

        try
        {
            monsterComponent.SpineController.Skeleton.SetSkin(productName.Split("_")[1]);
        }
        catch (Exception _)
        {
            Debug.LogError($"Monster의 skin 이름을 설정할 수 없습니다: {productName}");
        }

        var penaltyPlayingData = new EatingMonster(monsterComponent);
        _survivedMonsters.Add(penaltyPlayingData);
    }

    private void SpawnCropLockerAt(Vector2 cropWorldPosition, float lockTime)
    {
        // CropLock 오브젝트 스폰
        var cropLockObject = Instantiate(cropLockPrefab, transform);
        var cropLockComponent = cropLockObject.GetComponent<CropLock>();
        cropLockObject.transform.position = cropWorldPosition;
            
        var cropLocker = new CropLocker(cropLockComponent, lockTime);
        _cropLockers.Add(cropLocker);
    }
}
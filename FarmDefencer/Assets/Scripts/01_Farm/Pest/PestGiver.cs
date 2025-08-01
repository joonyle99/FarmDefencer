using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class PestGiver 
    : MonoBehaviour, 
    IFarmUpdatable,
    IFarmInputLayer,
    IFarmSerializable
{
    public int InputPriority => IFarmInputLayer.Priority_PestGiver;

    public bool IsWarningShowing => _pestWarningUI.IsWarningShowing;

    public bool IsPestRunning => _runningPests.Count > 0;
    
    [Serializable]
    public struct PestPrefabData
    {
        [SerializeField] private PestSize size;
        public PestSize Size => size;
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;
    }

    [Header("앞에서 출발한 해충과의 최소 시간 간격")] [SerializeField] private float minimumPestSpawnInterval = 0.1f;
    [Header("앞에서 출발한 해충과의 최대 시간 간격")] [SerializeField] private float maximumPestSpawnInterval = 1.0f;
    [Header("지그재그로 움직이다가 곧장 도착지로 향하기 시작하는 거리 기준")] [SerializeField] private float pestBeginDirectToDestinationCriterion = 5.0f;
    [Header("해충 클릭 판정되는 크기")] [SerializeField] private float pestClickSize = 0.1f;
    [Header("잡힌 해충 사라지는데 걸리는 시간")] [SerializeField] private float pestDieTime = 0.5f;
    [Header("다 훔쳐간 사라지는데 걸리는 시간")] [SerializeField] private float pestFleeTime = 1.0f;
    [Header("해충 웨이브 발생하는 최소 시각")] [SerializeField] private float minimumRandomPestSpawnDaytime = 30.0f;
    [Header("해충 웨이브 발생하는 최대 시각")] [SerializeField] private float maximumRandomPestSpawnDaytime = 200.0f;
    [Header("해충 지그재그 파장")] [SerializeField] private float wavelength = 1.0f;
    [Header("해충 지그재그 진폭")] [SerializeField] private float amplitude = 1.0f;
    [Header("수확한 작물 해충한테 날라가는데 걸리는 시간")] [SerializeField] private float stealAnimationDuration = 0.2f;
    [Header("해충 프리팹들")][SerializeField] private List<PestPrefabData> pestPrefabs;
    [Header("해충 스폰 규칙")] [SerializeField] private PestSpawnRule pestSpawnRule;
    [Header("해충 속도 규칙")] [SerializeField] private PestSpeedRule pestSpeedRule;

    private Func<string, bool> _isProductAvailable;
    private Func<string, ProductEntry> _getProductEntry;
    private Func<float> _getDaytime;
    private Func<bool> _isFarmPaused;
    private Action<int> _onPestCatchGoldEarned;
    private GameObject _runningPestParentObject;
    private Dictionary<PestOrigin, Vector2> _pestOrigins;
    private PestWarningUI _pestWarningUI;
    private float _remainingNextRunTime;
    private Action<ProductEntry, float, Vector2, Vector2> _playPestStealFromFieldAnimation;
    
    // 저장되는 값들
    private float _pestSpawnTime;
    private bool _isPestSpawnReserved;
    private List<Pest> _runningPests;
    private Dictionary<string, PestEatingPoint> _pestEatingPoints;

    public void Init(
        Func<string, bool> isProductAvailable, 
        Func<string, ProductEntry> getProductEntry,
        Func<float> getDaytime,
        Func<bool> isFarmPaused,
        Action<int> onPestCatchGoldEarned,
        Action<ProductEntry, float, Vector2, Vector2> playPestStealFromFieldAnimation)
    {
        _isProductAvailable = isProductAvailable;
        _getProductEntry = getProductEntry;
        _getDaytime = getDaytime;
        _isFarmPaused = isFarmPaused;
        _onPestCatchGoldEarned = onPestCatchGoldEarned;
        _playPestStealFromFieldAnimation = playPestStealFromFieldAnimation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="cropWorldPosition"></param>
    /// <param name="count">먹고 남은 개수.</param>
    /// <returns></returns>
    public int LetPestsEat(string productName, Vector2 cropWorldPosition, int count) => _pestEatingPoints[productName].LetPestsEat(cropWorldPosition, count);

    public void ReserveRandomPestSpawn()
    {
        _isPestSpawnReserved = true;
        _pestSpawnTime = Random.Range(minimumRandomPestSpawnDaytime, maximumRandomPestSpawnDaytime);
    }

    public void OnFarmUpdate(float deltaTime)
    {
        if (IsPestRunning)
        {
            SoundManager.Instance.PlayAmb("AMB_T_pest_sirenamb", SoundManager.Instance.ambVolume);    
        }
        else
        {
            SoundManager.Instance.StopAmbIf("AMB_T_pest_sirenamb");
        }
        
        if (_isPestSpawnReserved && _getDaytime() >= _pestSpawnTime)
        {
            _isPestSpawnReserved = false;
            SoundManager.Instance.PlaySfx("SFX_T_pest_siren", SoundManager.Instance.pestSirenVolume);
            _pestWarningUI.ShowWarning();
            var pestSpawnList = CreatePestSpawnRule(_isProductAvailable, pestSpawnRule);
            var availableProducts = GetAvailableTargetProducts(_isProductAvailable, _getProductEntry);

            foreach (var (pestOrigin, pestSize) in pestSpawnList)
            {
                var seed = Random.Range(0, 10000);
                var target = SelectTarget(pestOrigin, availableProducts);
                if (target is null)
                {
                    Debug.LogError($"PestOrigin {pestOrigin}, PestSize {pestSize} 에 대한 해충 타겟을 찾을 수 없습니다.");
                    continue;
                }

                var speedRuleEntry = pestSpeedRule.Rule.FirstOrDefault(entry =>
                    entry.PreconditionCurrentMap.MapId == MapManager.Instance.CurrentMapIndex);
                var speedRange = pestSize switch
                {
                    PestSize.Small => speedRuleEntry.SmallPestSpeedRange,
                    PestSize.Medium => speedRuleEntry.MediumPestSpeedRange,
                    _ => speedRuleEntry.LargePestSpeedRange
                };
                var speed = Random.Range(speedRange.x, speedRange.y);

                var pestComponent = InstantiatePest(pestSize);
                pestComponent.transform.parent = _runningPestParentObject.transform;
                pestComponent.transform.position = _pestOrigins[pestOrigin];
                pestComponent.SetParameters(seed, target.ProductName, _pestEatingPoints[target.ProductName].transform.position, speed);
                _runningPests.Add(pestComponent);
            }
        }

        if (_runningPests.Count > 0 && _runningPests.Any(p => p.State == PestState.Initialized))
        {
            _remainingNextRunTime -= deltaTime;
            if (_remainingNextRunTime <= 0.0f)
            {
                _remainingNextRunTime = Random.Range(minimumPestSpawnInterval, maximumPestSpawnInterval);
                var firstPest = _runningPests.OrderBy(p => p.Seed).First(p => p.State == PestState.Initialized);
                firstPest.Run();
            }
        }
        
        _runningPests.ForEach(p => p.ManualUpdate(deltaTime));
        foreach (var arrivedPest in _runningPests.Where(p => p.State == PestState.Arrived))
        {
            var pestEatingPoint = _pestEatingPoints[arrivedPest.TargetProduct];
            pestEatingPoint.AddArrivedPest(arrivedPest);
        }

        _runningPests.RemoveAll(p => p.State == PestState.Arrived);
    }

    public bool OnTap(Vector2 worldPosition)
    {
        if (_isFarmPaused())
        {
            return false;
        }
        
        var pest = _runningPests.Find(p =>
        {
            var distanceSquared =
                new Vector2(p.transform.position.x - worldPosition.x, p.transform.position.y - worldPosition.y)
                    .sqrMagnitude;
            return distanceSquared < pestClickSize * pestClickSize && p.State == PestState.Running;
        });
        if (pest is null)
        {
            return false;
        }
        
        pest.Hit();
        if (pest.State != PestState.Dying)
        {
            return true;
        }

        _runningPests.Remove(pest);
        var gold = pest.PestSize switch
        {
            PestSize.Large => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _onPestCatchGoldEarned(gold);

        return true;
    }

    public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime) => false;
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("PestSpawnTime", _pestSpawnTime);
        jsonObject.Add("IsPestSpawnReserved", _isPestSpawnReserved);

        var jsonRunningPests = new JArray();
        foreach (var runningPest in _runningPests)
        {
            jsonRunningPests.Add(runningPest.Serialize());
        }
        jsonObject.Add("RunningPests", jsonRunningPests);

        var jsonPestEatingPoints = new JObject();
        foreach (var (targetProductName, pestEatingPoint) in _pestEatingPoints)
        {
            jsonPestEatingPoints.Add(targetProductName, pestEatingPoint.Serialize());
        }
        jsonObject.Add("PestEatingPoints", jsonPestEatingPoints);

        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        _pestSpawnTime = json["PestSpawnTime"]?.Value<int>() ?? 0.0f;
        _isPestSpawnReserved = json["IsPestSpawnReserved"]?.Value<bool>() ?? false;

        if (json["RunningPests"] is JArray jsonRunningPests)
        {
            foreach (var jsonRunningPestToken in jsonRunningPests)
            {
                var jsonRunningPest = jsonRunningPestToken as JObject;
                if (jsonRunningPest is null || jsonRunningPest["PestSize"]?.Value<int>() is null)
                {
                    continue;
                }

                var pestSize = (PestSize)jsonRunningPest["PestSize"].Value<int>();

                var runningPest = InstantiatePest(pestSize);
                _runningPests.Add(runningPest);
                runningPest.Deserialize(jsonRunningPest);
            }
        }

        if (json["PestEatingPoints"] is JObject jsonPestEatingPoints)
        {
            foreach (var jsonPestEatingPointProperty in jsonPestEatingPoints.Values<JProperty>())
            {
                var targetProductName = jsonPestEatingPointProperty.Name;
                if (!_pestEatingPoints.TryGetValue(targetProductName, out var pestEatingPoint)
                    || jsonPestEatingPointProperty.Value is not JObject jsonPestEatingPoint)
                {
                    continue;
                }
                pestEatingPoint.Deserialize(jsonPestEatingPoint);
            }
        }
    }

    private void Awake()
    {
        _pestEatingPoints = new Dictionary<string, PestEatingPoint>();
        _pestOrigins = new Dictionary<PestOrigin, Vector2>();
        _runningPestParentObject = transform.Find("RunningPests").gameObject;
        _runningPests = new List<Pest>();
        _pestWarningUI = transform.Find("PestWarningUI").GetComponent<PestWarningUI>();

        var pestEatingPointsObject = transform.Find("PestEatingPoints");
        for (int i = 0; i < pestEatingPointsObject.childCount; ++i)
        {
            var pestEatingPoint = pestEatingPointsObject.GetChild(i).GetComponent<PestEatingPoint>();
            pestEatingPoint.Init(InstantiatePest);
            _pestEatingPoints.Add(pestEatingPoint.TargetProduct.ProductName, pestEatingPoint);
        }

        _pestOrigins.Add(PestOrigin.Left, transform.Find("PestOrigins/Left").position);
        _pestOrigins.Add(PestOrigin.Right, transform.Find("PestOrigins/Right").position);
    }

    private Pest InstantiatePest(PestSize pestSize)
    {
        var pestPrefab = pestPrefabs.Find(data => data.Size == pestSize).Prefab;
        var pestObject = Instantiate(pestPrefab, _runningPestParentObject.transform);
        pestObject.name = $"Pest_{pestSize}";

        var pestComponent = pestObject.GetComponent<Pest>();
        pestComponent.Init(
            pestSize,
            pestDieTime,
            pestFleeTime,
            pestBeginDirectToDestinationCriterion,
            wavelength,
            amplitude,
            (targetProductName, worldFrom, worldTo) =>
            {
                var targetProduct = _getProductEntry(targetProductName);
                _playPestStealFromFieldAnimation(targetProduct, stealAnimationDuration, worldFrom, worldTo);
            });

        return pestComponent;
    }

    private static List<(PestOrigin, PestSize)> CreatePestSpawnRule(Func<string, bool> isProductAvailable, PestSpawnRule pestSpawnRule)
    {
        var selectedEntry = pestSpawnRule.Rule.Reverse()
            .FirstOrDefault(entry => isProductAvailable(entry.PreconditionEnabledProduct.ProductName));

        return selectedEntry.ToList();
    }

    private static List<Tuple<PestOrigin, ProductEntry>> GetAvailableTargetProducts(
        Func<string, bool> isProductAvailable, Func<string, ProductEntry> getProductEntry)
    {
        var availableProducts = new List<Tuple<PestOrigin, ProductEntry>>();

        if (isProductAvailable("product_carrot"))
            availableProducts.Add(new(PestOrigin.Right, getProductEntry("product_carrot")));
        if (isProductAvailable("product_corn"))
            availableProducts.Add(new(PestOrigin.Right, getProductEntry("product_corn")));
        if (isProductAvailable("product_potato"))
            availableProducts.Add(new(PestOrigin.Right, getProductEntry("product_potato")));

        if (isProductAvailable("product_cabbage"))
            availableProducts.Add(new(PestOrigin.Left, getProductEntry("product_cabbage")));
        if (isProductAvailable("product_eggplant"))
            availableProducts.Add(new(PestOrigin.Left, getProductEntry("product_eggplant")));
        if (isProductAvailable("product_cucumber"))
            availableProducts.Add(new(PestOrigin.Left, getProductEntry("product_cucumber")));
        if (isProductAvailable("product_sweetpotato"))
            availableProducts.Add(new(PestOrigin.Left, getProductEntry("product_sweetpotato")));
        if (isProductAvailable("product_mushroom"))
            availableProducts.Add(new(PestOrigin.Left, getProductEntry("product_mushroom")));

        return availableProducts;
    }

    private static ProductEntry SelectTarget(PestOrigin origin,
        List<Tuple<PestOrigin, ProductEntry>> availableTargetProducts)
    {
        ProductEntry target = null;

        int count = 0;
        foreach (var (pestOrigin, product) in availableTargetProducts)
        {
            if (pestOrigin != origin) continue;

            count += 1;
            if (Random.Range(0, count) == 0)
            {
                target = product;
            }
        }

        return target;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

public sealed class PestGiver 
    : MonoBehaviour, 
    IFarmUpdatable,
    IFarmInputLayer,
    IFarmSerializable
{
    private enum PestSpawnState
    {
        NotRequested,
        Requested,
        Spawned
    }
    
    public int InputPriority => IFarmInputLayer.Priority_PestGiver;

    public bool IsWarningShowing => _pestWarningUI.IsWarningShowing;

    public bool ShouldReservePestSpawn => _pestSpawnState == PestSpawnState.NotRequested;

    [Serializable]
    public struct PestPrefabData
    {
        [SerializeField] private PestSize size;
        public PestSize Size => size;
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;
    }

    [Tooltip("해충 뛰는 속도")] [SerializeField] private float pestMoveSpeed = 10.0f;
    [Tooltip("앞에서 출발한 해충과의 최소 시간 간격")] [SerializeField] private float minimumPestSpawnInterval = 0.1f;
    [Tooltip("앞에서 출발한 해충과의 최대 시간 간격")] [SerializeField] private float maximumPestSpawnInterval = 1.0f;
    [Tooltip("지그재그로 움직이다가 곧장 도착지로 향하기 시작하는 거리 기준")] [SerializeField] private float pestBeginDirectToDestinationCriterion = 5.0f;
    [SerializeField] private float distanceBetweenArrivedPests = 0.2f;
    [Tooltip("해충 클릭 판정되는 크기")] [SerializeField] private float pestClickSize = 0.1f; // 해충 클릭 판정되는 크기.
    [Tooltip("잡힌 해충 사라지는데 걸리는 시간")] [SerializeField] private float pestDieTime = 0.5f; // 해충 잡았을 때 투명해져 완전히 사라질때까지 걸리는 시간.
    [Tooltip("해충 웨이브 발생하는 최소 시각")] [SerializeField] private float minimumRandomPestSpawnDaytime = 30.0f;
    [Tooltip("해충 웨이브 발생하는 최대 시각")] [SerializeField] private float maximumRandomPestSpawnDaytime = 200.0f;
    [Tooltip("해충 프리팹들")][SerializeField] private List<PestPrefabData> pestPrefabs;
    [Header("해충 스폰 규칙")] [SerializeField] private PestSpawnRule pestSpawnRule;

    private Func<string, bool> _isProductAvailable;
    private Func<string, ProductEntry> _getProductEntry;
    private Func<float> _getDaytime;
    private Action<int> _onPestCatchGoldEarned;
    private GameObject _runningPestParentObject;
    private Dictionary<PestOrigin, Vector2> _pestOrigins;
    private PestWarningUI _pestWarningUI;
    private float _remainingNextRunTime;
    
    // 저장되는 값들
    private float _pestSpawnTime;
    private PestSpawnState _pestSpawnState;
    private List<Pest> _runningPests;
    private Dictionary<string, PestEatingPoint> _pestEatingPoints;

    public void Init(
        Func<string, bool> isProductAvailable, 
        Func<string, ProductEntry> getProductEntry,
        Func<float> getDaytime,
        Action<int> onPestCatchGoldEarned)
    {
        _isProductAvailable = isProductAvailable;
        _getProductEntry = getProductEntry;
        _getDaytime = getDaytime;
        _onPestCatchGoldEarned = onPestCatchGoldEarned;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="count">먹고 남은 개수.</param>
    /// <returns></returns>
    public int LetPestsEat(string productName, int count) => _pestEatingPoints[productName].LetPestsEat(count);

    public void ReserveRandomPestSpawn()
    {
        _pestSpawnState = PestSpawnState.Requested;
        _pestSpawnTime = Random.Range(minimumRandomPestSpawnDaytime, maximumPestSpawnInterval);
    }

    public void OnFarmUpdate(float deltaTime)
    {
        if (_pestSpawnState == PestSpawnState.Requested && _getDaytime() >= _pestSpawnTime)
        {
            _pestSpawnState = PestSpawnState.Spawned;
            
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

                var pestComponent = InstantiatePest(pestSize);
                pestComponent.transform.parent = _runningPestParentObject.transform;
                pestComponent.transform.position = _pestOrigins[pestOrigin];
                pestComponent.SetParameters(seed, target.ProductName, _pestEatingPoints[target.ProductName].transform.position);
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

    public bool OnSingleTap(Vector2 worldPosition)
    {
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

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime) => false;
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("PestSpawnTime", _pestSpawnTime);
        jsonObject.Add("PestSpawnState", (int)_pestSpawnState);

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
        _pestSpawnState = (PestSpawnState)(json["PestSpawnState"]?.Value<int>() ?? 0);

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

        var pestComponent = pestObject.AddComponent<Pest>();
        pestComponent.Init(
            pestSize,
            pestMoveSpeed,
            pestDieTime,
            pestBeginDirectToDestinationCriterion);

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
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
    public int InputPriority => IFarmInputLayer.Priority_PestGiver;

    public bool IsWarningShowing => _pestWarningUI.IsWarningShowing;

    [Serializable]
    public struct PestPrefabData
    {
        [SerializeField] private PestSize size;
        public PestSize Size => size;
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;
    }

    [SerializeField] private float pestMoveSpeed = 10.0f;
    [SerializeField] private float minimumPestSpawnInterval = 0.1f;
    [SerializeField] private float maximumPestSpawnInterval = 1.0f;
    [SerializeField] private float pestBeginDirectToDestinationCriterion = 5.0f; // 달리는 중인 해충의 도착지와의 거리값이 이 값보다 작으면 지그재그 이동 대신 목적지로 직접 향하기 시작함.
    [SerializeField] private float distanceBetweenArrivedPests = 0.2f;
    [SerializeField] private float pestClickSize = 0.1f; // 해충 클릭 판정되는 크기.
    [SerializeField] private float pestDieTime = 0.5f; // 해충 잡았을 때 투명해져 완전히 사라질때까지 걸리는 시간.
    [SerializeField] private float pestWarningDuration = 3.0f;
    [SerializeField] private List<PestPrefabData> pestPrefabs;

    private Func<string, bool> _isProductAvailable;
    private Func<string, ProductEntry> _getProductEntry;
    private Func<float> _getDaytime;
    private Action<int> _onPestCatchGoldEarned;
    private GameObject _pestsObject;
    private Dictionary<PestOrigin, Vector2> _pestOrigins;
    private PestWarningUI _pestWarningUI;
    private float _remainingNextRunTime;
    
    // 저장되는 값들
    private float _pestSpawnTime;
    private bool _shouldSpawnPests;
    private List<Pest> _runningPests;
    private Dictionary<string, PestDestination> _pestDestinations;

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
    public int LetPestsEat(string productName, int count) => _pestDestinations[productName].LetPestsEat(count);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pestSpawnTime">낮 시간 기준으로 몇 초에 스폰할지.</param>
    public void SpawnPestsAt(float pestSpawnTime)
    {
        _shouldSpawnPests = true;
        _pestSpawnTime = pestSpawnTime;
    }

    public void OnFarmUpdate(float deltaTime)
    {
        if (_shouldSpawnPests && _getDaytime() >= _pestSpawnTime)
        {
            _shouldSpawnPests = false;
            
            _pestWarningUI.ShowWarning();
            var pestSpawnRule = CreatePestSpawnRule(_isProductAvailable);
            var availableProducts = GetAvailableTargetProducts(_isProductAvailable, _getProductEntry);

            foreach (var (pestOrigin, pestSize) in pestSpawnRule)
            {
                var seed = Random.Range(0, 10000);
                var target = SelectTarget(pestOrigin, availableProducts);
                if (target is null)
                {
                    Debug.LogError($"PestOrigin {pestOrigin}, PestSize {pestSize} 에 대한 해충 타겟을 찾을 수 없습니다.");
                    continue;
                }

                var pestComponent = InstantiatePest(pestSize, pestOrigin, target, seed);
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
            var pestDestination = _pestDestinations[arrivedPest.TargetProduct.ProductName];
            pestDestination.AddPest(arrivedPest);
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
            PestSize.Big => 3,
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
        jsonObject.Add("ShouldSpawnPests", _shouldSpawnPests);

        var jsonRunningPests = new JArray();
        foreach (var runningPest in _runningPests)
        {
            jsonRunningPests.Add(runningPest.Serialize());
        }
        jsonObject.Add("RunningPests", jsonRunningPests);

        var jsonArrivedPests = new JObject();
        foreach (var (targetProductName, pestDestination) in _pestDestinations)
        {
            var arrivedPests = pestDestination.Pests.Reverse(); // AddPest()가 앞에 채워넣을 것이므로, 역직렬화시를 대비해 역순으로 직렬화함.
            var jsonArrivedPestsPerProduct = new JArray();
            foreach (var arrivedPest in arrivedPests)
            {
                jsonArrivedPestsPerProduct.Add(arrivedPest.Serialize());
            }
            jsonArrivedPests.Add(targetProductName, jsonArrivedPestsPerProduct);
        }
        jsonObject.Add("ArrivedPests", jsonArrivedPests);

        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        _pestSpawnTime = json.Property("PestSpawnTime")?.Value.Value<int>() ?? 0.0f;
        _shouldSpawnPests = json.Property("ShouldSpawnPests")?.Value.Value<bool>() ?? false;

        if (json["RunningPests"] is JArray jsonRunningPests)
        {
            foreach (var jsonRunningPestToken in jsonRunningPests)
            {
                Debug.Log($"{jsonRunningPestToken.ToString()}");
                var jsonRunningPest = jsonRunningPestToken as JObject;
                if (jsonRunningPest is null)
                {
                    continue;
                }

                var runningPest = InstantiatePest(jsonRunningPest);
                if (runningPest is not null)
                {
                    _runningPests.Add(runningPest);
                    runningPest.Deserialize(jsonRunningPest);
                }
            }
        }

        if (json["ArrivedPests"] is JObject jsonArrivedPests)
        {
            foreach (var jsonPestDestination in jsonArrivedPests.Values<JProperty>())
            {
                var targetProductName = jsonPestDestination.Name;
                var jsonArrivedPestsPerProduct = jsonPestDestination.Value as JArray;
                if (jsonArrivedPestsPerProduct is null || !_pestDestinations.TryGetValue(targetProductName, out var pestDestination))
                {
                    continue;
                }

                foreach (var jsonArrivedPestToken in jsonArrivedPestsPerProduct)
                {
                    var jsonArrivedPest = jsonArrivedPestToken as JObject;
                    if (jsonArrivedPest is null)
                    {
                        continue;
                    }

                    var arrivedPest = InstantiatePest(jsonArrivedPest);
                    pestDestination.AddPest(arrivedPest);
                }
            }
        }
    }

    private void Awake()
    {
        _pestDestinations = new Dictionary<string, PestDestination>();
        _pestOrigins = new Dictionary<PestOrigin, Vector2>();
        _pestsObject = transform.Find("Pests").gameObject;
        _runningPests = new List<Pest>();
        _pestWarningUI = transform.Find("PestWarningUI").GetComponent<PestWarningUI>();

        var pestDestinationsObject = transform.Find("PestDestinations");
        for (int i = 0; i < pestDestinationsObject.childCount; ++i)
        {
            var pestDestinationComponent = pestDestinationsObject.GetChild(i).GetComponent<PestDestination>();
            _pestDestinations.Add(pestDestinationComponent.TargetProduct.ProductName, pestDestinationComponent);
            pestDestinationComponent.Init(distanceBetweenArrivedPests);
        }

        _pestOrigins.Add(PestOrigin.Left, transform.Find("PestOrigins/Left").position);
        _pestOrigins.Add(PestOrigin.Right, transform.Find("PestOrigins/Right").position);
    }

    private Pest InstantiatePest(PestSize pestSize, PestOrigin pestOrigin, ProductEntry target, int seed)
    {
        var pestPrefab = pestPrefabs.Find(data => data.Size == pestSize).Prefab;
        var pestObject = Instantiate(pestPrefab, _pestsObject.transform);
        pestObject.name = $"Pest_{target.ProductName}_{pestSize}";
        pestObject.transform.position = _pestOrigins[pestOrigin];
        pestObject.transform.parent = _pestsObject.transform;

        var pestComponent = pestObject.AddComponent<Pest>();
        pestComponent.Init(
            pestSize,
            pestOrigin,
            seed,
            target,
            _pestDestinations[target.ProductName].transform.position,
            pestMoveSpeed,
            pestBeginDirectToDestinationCriterion,
            pestDieTime);

        return pestComponent;
    }
    
    private Pest InstantiatePest(JObject jsonObject)
    {
        var pestSize = jsonObject["PestSize"]?.Value<int>();
        var pestOrigin = jsonObject["PestOrigin"]?.Value<int>();
        var targetProductName = jsonObject["TargetProduct"]?.Value<string>();
        var seed = jsonObject["Seed"]?.Value<int>();
        if (pestSize is null || pestOrigin is null || targetProductName is null || seed is null)
        {
            return null;
        }

        var pest = InstantiatePest((PestSize)pestSize.Value, (PestOrigin)pestOrigin.Value, _getProductEntry(targetProductName), seed.Value);
        return pest;
    }

    private static List<Tuple<PestOrigin, PestSize>> CreatePestSpawnRule(Func<string, bool> isProductAvailable)
    {
        if (isProductAvailable("product_mushroom"))
        {
            return generateList(5, 5, 5, 5, 5, 5);
        }

        if (isProductAvailable("product_sweetpotato"))
        {
            return generateList(4, 4, 5, 4, 5, 5);
        }

        if (isProductAvailable("product_eggplant"))
        {
            return generateList(4, 4, 4, 4, 4, 4);
        }

        if (isProductAvailable("product_cucumber"))
        {
            return generateList(3, 3, 3, 4, 3, 4);
        }

        if (isProductAvailable("product_cabbage"))
        {
            return generateList(1, 2, 2, 2, 4, 3);
        }

        return generateList(0, 0, 0, 3, 5, 5);

        List<Tuple<PestOrigin, PestSize>> generateList(int remainingLeftLarge, int remainingLeftMedium,
            int remainingLeftSmall, int remainingRightLarge, int remainingRightMedium, int remainingRightSmall)
        {
            var spawnRule = new List<Tuple<PestOrigin, PestSize>>();

            for (int i = 0; i < remainingLeftLarge; i++) spawnRule.Add(new(PestOrigin.Left, PestSize.Big));
            for (int i = 0; i < remainingLeftMedium; i++) spawnRule.Add(new(PestOrigin.Left, PestSize.Medium));
            for (int i = 0; i < remainingLeftSmall; i++) spawnRule.Add(new(PestOrigin.Left, PestSize.Small));
            for (int i = 0; i < remainingRightLarge; i++) spawnRule.Add(new(PestOrigin.Right, PestSize.Big));
            for (int i = 0; i < remainingRightMedium; i++) spawnRule.Add(new(PestOrigin.Right, PestSize.Medium));
            for (int i = 0; i < remainingRightSmall; i++) spawnRule.Add(new(PestOrigin.Right, PestSize.Small));

            return spawnRule;
        }
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
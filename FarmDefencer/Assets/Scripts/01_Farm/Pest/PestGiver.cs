using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;

public sealed class PestGiver : MonoBehaviour, IFarmUpdatable, IFarmInputLayer
{
    public int InputPriority => IFarmInputLayer.Priority_PestGiver;

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
    [SerializeField] private List<PestPrefabData> pestPrefabs;

    private Func<string, bool> _isProductAvailable;
    private Func<string, ProductEntry> _getProductEntry;
    private Action<int> _onPestCatchGoldEarned;
    private GameObject _pestsObject;
    private Dictionary<string, PestDestination> _pestDestinations;
    private Dictionary<PestOrigin, Vector2> _pestOrigins;
    private float _pestSpawnTime;
    private bool _isPestSpawned;

    private List<Pest> _runningPests;

    public void Init(Func<string, bool> isProductAvailable, Func<string, ProductEntry> getProductEntry, Action<int> onPestCatchGoldEarned)
    {
        _isProductAvailable = isProductAvailable;
        _getProductEntry = getProductEntry;
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
        _isPestSpawned = false;
        _pestSpawnTime = pestSpawnTime;
    }

    public void OnFarmUpdate(float deltaTime)
    {
        if (!_isPestSpawned)
        {
            _pestSpawnTime -= deltaTime;
            if (_pestSpawnTime <= 0)
            {
                _isPestSpawned = true;
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

                    var pestPrefab = pestPrefabs.Find(data => data.Size == pestSize).Prefab;
                    var pestObject = Instantiate(pestPrefab, _pestsObject.transform);
                    pestObject.name = $"Pest_{target.ProductName}_{pestSize}";
                    pestObject.transform.position = _pestOrigins[pestOrigin];

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
                    _runningPests.Add(pestComponent);
                }

                StartCoroutine(DoRunPests());
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

    private void Awake()
    {
        _pestDestinations = new Dictionary<string, PestDestination>();
        _pestOrigins = new Dictionary<PestOrigin, Vector2>();
        _pestsObject = transform.Find("Pests").gameObject;
        _runningPests = new List<Pest>();

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

    private IEnumerator DoRunPests()
    {
        foreach (var pest in _runningPests.OrderBy(p => p.Seed))
        {
            pest.Run();
            var randomInterval = Random.Range(minimumPestSpawnInterval, maximumPestSpawnInterval);
            yield return new WaitForSeconds(randomInterval);
        }

        yield return null;
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
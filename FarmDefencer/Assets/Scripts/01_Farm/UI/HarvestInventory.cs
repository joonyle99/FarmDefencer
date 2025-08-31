using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

/// <summary>
/// 자식으로 HarvestAnimationPlayer 오브젝트를 가져야 함.
/// </summary>
public sealed class HarvestInventory : MonoBehaviour, IFarmSerializable
{
    [InfoBox("작물 한 뭉치가 얼마의 시간 내에 다 날아들어 가야 하는지")]
    [SerializeField] private float harvestBulkAnimationTime = 0.25f;

    [InfoBox("시작점 작물 출발 위치를 얼마나 흩뿌릴지")] [SerializeField] private float harvestBulkSpread = 0.5f;
    [SerializeField] private float harvestedProductFlyToBoxAnimationDuration = 0.25f;
    [SerializeField] private float blinkDuration = 0.5f;

    [CanBeNull] public ProductEntry SpecialProduct { get; private set; }
    [CanBeNull] public ProductEntry HotProduct { get; private set; }

    [SerializeField] private int specialProductTurnInterval = 4;

    [SerializeField] private QuotaFillingRule quotaFillingRule;
    [SerializeField] private CropUnlockRule cropUnlockRule;

    private Canvas _canvas;
    private Dictionary<ProductEntry, HarvestBox> _harvestBoxes;
    private int _hotSpecialTurn;

    private Func<string, ProductEntry> _getProductEntry;

    public JObject Serialize()
    {
        var json = new JObject();

        json.Add(new JProperty("Turn", _hotSpecialTurn));

        if (HotProduct is not null)
        {
            json.Add(new JProperty("HotProduct", HotProduct.ProductName));
        }

        if (SpecialProduct is not null)
        {
            json.Add(new JProperty("SpecialProduct", SpecialProduct.ProductName));
        }

        var jsonHarvestBoxes = new JObject();
        foreach (var (productEntry, harvestBox) in _harvestBoxes)
        {
            jsonHarvestBoxes.Add(productEntry.ProductName, harvestBox.Quota);
        }

        json.Add("HarvestBoxes", jsonHarvestBoxes);

        return json;
    }

    public void Deserialize(JObject json)
    {
        if (json["HarvestBoxes"] is JObject jsonHarvestBoxes)
        {
            foreach (var jsonHarvestBox in jsonHarvestBoxes.Properties())
            {
                var harvestBox = _harvestBoxes.FirstOrDefault(pair => pair.Key.ProductName.Equals(jsonHarvestBox.Name))
                    .Value;
                harvestBox.Deserialize(jsonHarvestBox.Value<JObject>());
            }
        }

        _hotSpecialTurn = json.Property("Turn")?.Value.Value<int>() ?? 0;
        HotProduct = json.Property("HotProduct")?.Value.Value<string>() is string hotProductName
            ? _getProductEntry(hotProductName)
            : null;
        SpecialProduct = json.Property("SpecialProduct")?.Value.Value<string>() is string specialProductName
            ? _getProductEntry(specialProductName)
            : null;
    }

    public void Init(Func<string, ProductEntry> getProductEntry)
    {
        _getProductEntry = getProductEntry;
    }

    public bool IsProductAvailable(string productName, int currentMapId, int currentStageId) => cropUnlockRule.IsCropUnlocked(productName, currentMapId, currentStageId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="cropWorldPosition"></param>
    /// <param name="count"></param>
    /// <param name="currentMapId"></param>
    /// <param name="currentStageId"></param>
    /// <param name="onGolearnGolddEarned"></param>
    /// <param name="goldEarnEffect"></param>
    public void SellProduct(
        string productName, 
        Vector2 cropWorldPosition, 
        int count,
        int currentMapId,
        int currentStageId,
        Action<int> earnGold,
        Action<ProductEntry, float, Vector2, Vector2> playScreenHarvestAnimation,
        GoldEarnEffect goldEarnEffect)
    {
        var harvestBox = _harvestBoxes.FirstOrDefault(pair => pair.Key.ProductName.Equals(productName)).Value;
        StartCoroutine(CoSellProduct(harvestBox, cropWorldPosition, count, currentMapId, currentStageId, earnGold, playScreenHarvestAnimation, goldEarnEffect));
    }

    public void AssignAllQuotas(int currentMapId, int currentStageId)
    {
        foreach (var productEntry in _harvestBoxes.Keys)
        {
            AssignQuotaOf(productEntry.ProductName, currentMapId, currentStageId);
            _hotSpecialTurn = 0;
        }
    }

    public void AssignIfJustUnlocked(int currentMapId, int currentStageId)
    {
        foreach (var harvestBox in _harvestBoxes.Values)
        {
            if (harvestBox.Quota <= 0 &&
                cropUnlockRule.IsCropUnlocked(harvestBox.ProductEntry.ProductName, currentMapId, currentStageId))
            {
                AssignQuotaOf(harvestBox.ProductEntry.ProductName, currentMapId, currentStageId);
            }
        }
    }

    public void AssignQuotaOf(string productName, int currentMapId, int currentStageId)
    {
        var harvestBox = _harvestBoxes.FirstOrDefault(pair => pair.Key.ProductName.Equals(productName)).Value;
        harvestBox.Quota = 0;

        var ruleForMap = quotaFillingRule.Entries.FirstOrDefault(r => r.TargetMap.MapId == currentMapId);
        if (ruleForMap is null)
        {
            Debug.LogError($"{currentMapId} 에 해당하는 QuotaFillingRuleForMap을 찾을 수 없습니다.");
            return;
        }

        if (!cropUnlockRule.IsCropUnlocked(harvestBox.ProductEntry.ProductName, currentMapId, currentStageId))
        {
            return;
        }

        var (minimum, maximum) = ruleForMap.Entries
            .FirstOrDefault(entry => entry.TargetProduct.ProductName.Equals(productName)).Range;

        harvestBox.Quota = Random.Range(minimum, maximum + 1) / 10 * 10;

        // 핫, 스페셜 배정해야 할 때가 아니면 스킵
        if (HotProduct is not null && !HotProduct.ProductName.Equals(productName) ||
            SpecialProduct is not null && !SpecialProduct.ProductName.Equals(productName))
        {
            return;
        }

        foreach (var box in _harvestBoxes.Values)
        {
            box.ClearSpecialOrHot();
        }
        
        HotProduct = null;
        SpecialProduct = null;

        _hotSpecialTurn = (_hotSpecialTurn + 1) % specialProductTurnInterval;

        var isSpecialTurn = _hotSpecialTurn == specialProductTurnInterval - 1;

        var quotaSum = 0;
        foreach (var box in _harvestBoxes.Values)
        {
            quotaSum += box.Quota;
        }

        var random = Random.Range(0, quotaSum);
        foreach (var box in _harvestBoxes.Values)
        {
            random -= box.Quota;
            if (random < 0)
            {
                if (isSpecialTurn)
                {
                    SpecialProduct = box.ProductEntry;
                    box.MarkSpecial();
                    
                }
                else
                {
                    HotProduct = box.ProductEntry;
                    box.MarkHot();
                }
                break;
            }
        }
    }

    private void Awake()
    {
        _harvestBoxes = new();
        _canvas = transform.Find("Canvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        var boxAreaObject = transform.Find("Canvas/Drawer/BoxArea").gameObject;
        for (var childIndex = 0; childIndex < boxAreaObject.transform.childCount; ++childIndex)
        {
            var child = boxAreaObject.transform.GetChild(childIndex).gameObject;
            var harvestBox = child.GetComponent<HarvestBox>();
            harvestBox.Init(blinkDuration);
            _harvestBoxes.Add(harvestBox.ProductEntry, harvestBox);
        }
    }

    private IEnumerator CoSellProduct(HarvestBox harvestBox, Vector2 cropWorldPosition, int count, int currentMapId, int currentStageId, Action<int> earnGold, Action<ProductEntry, float, Vector2, Vector2> playScreenHarvestAnimation, GoldEarnEffect goldEarnEffect)
    {
        var interval = harvestBulkAnimationTime / count;
        var cropWorldPosition3 = new Vector3(cropWorldPosition.x, cropWorldPosition.y, 90.0f);
        Vector2 cropScreenPosition = Camera.main.WorldToScreenPoint(cropWorldPosition3);
        Vector2 harvestBoxScreenPosition = _canvas.worldCamera.WorldToScreenPoint(harvestBox.transform.position);

        goldEarnEffect.PlayEffect(0);
        while (count > 0)
        {
            var price = harvestBox.ProductEntry.Price;
            if (harvestBox.ProductEntry == HotProduct)
            {
                price *= 2;
            }
            else if (harvestBox.ProductEntry == SpecialProduct && harvestBox.Quota == 1)
            {
                var ruleForMap = quotaFillingRule.Entries.FirstOrDefault(r => r.TargetMap.MapId == currentMapId);
                if (ruleForMap is not null &&
                    ruleForMap.SpecialProductBonusRule.TryGetBonusOf(harvestBox.ProductEntry.ProductName,
                        out var bonusPrice))
                {
                    price += bonusPrice;
                }
                else
                {
                    Debug.LogError($"{currentMapId} 에 해당하는 Special Product Bonus Rule을 찾을 수 없습니다.");
                }
            }

            if (count % 2 == 0)
            {
                var spread = new Vector2(Random.Range(-harvestBulkSpread, harvestBulkSpread),
                    Random.Range(-harvestBulkSpread, harvestBulkSpread));

                playScreenHarvestAnimation(harvestBox.ProductEntry,
                    harvestedProductFlyToBoxAnimationDuration,
                    cropScreenPosition + spread,
                    harvestBoxScreenPosition);
            }

            if (count % 10 == 0)
            {
                SoundManager.Instance.PlaySfx("SFX_T_coin");
            }

            harvestBox.Quota -= 1;
            count -= 1;
            if (harvestBox.Quota <= 0)
            {
                AssignQuotaOf(harvestBox.ProductEntry.ProductName, currentMapId, currentStageId);
            }

            earnGold(price);
            goldEarnEffect.transform.position = cropWorldPosition;
            goldEarnEffect.GoldDisplaying += price;

            yield return new WaitForSeconds(interval);
        }
    }
}
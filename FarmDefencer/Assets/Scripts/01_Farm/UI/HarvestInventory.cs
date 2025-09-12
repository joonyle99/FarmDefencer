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

    [InfoBox("시작점 작물 출발 위치를 얼마나 흩뿌릴지")] [SerializeField] private float harvestBulkSpread = 2.5f;
    [SerializeField] private float harvestedProductFlyToBoxAnimationDuration = 1.0f;
    [SerializeField] private float blinkDuration = 0.5f;

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

        json.Add(new JProperty("HotSpecialTurn", _hotSpecialTurn));

        var jsonHarvestBoxes = new JObject();
        foreach (var (productEntry, harvestBox) in _harvestBoxes)
        {
            jsonHarvestBoxes.Add(productEntry.ProductName, harvestBox.Serialize());
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
                var harvestBox = _harvestBoxes.FirstOrDefault(pair => pair.Key.ProductName == jsonHarvestBox.Name).Value;
                harvestBox.Deserialize(jsonHarvestBox.Value.Value<JObject>());
            }
        }

        _hotSpecialTurn = json.Property("HotSpecialTurn")?.Value.Value<int>() ?? 0;
    }

    public void Init(Func<string, ProductEntry> getProductEntry, int maxMapIndex, int maxStageIndex)
    {
        _getProductEntry = getProductEntry;
        var ruleForMap = quotaFillingRule.Entries.FirstOrDefault(r => r.TargetMap.MapId == maxMapIndex);
        if (ruleForMap is null)
        {
            Debug.LogError($"{maxMapIndex} 에 해당하는 QuotaFillingRuleForMap을 찾을 수 없습니다.");
            return;
        }
        
        foreach (var harvestBox in _harvestBoxes.Values)
        {
            var isCropUnlocked = cropUnlockRule.IsCropUnlocked(harvestBox.ProductEntry.ProductName, maxMapIndex, maxStageIndex);

            var minimum = 0;
            var maximum = 0;
            var specialBonusPrice = 0;
            if (isCropUnlocked)
            {
                var range = ruleForMap.Entries.First(entry => entry.TargetProduct.ProductName == harvestBox.ProductEntry.ProductName).Range;
                minimum = range.Item1;
                maximum = range.Item2;
                ruleForMap.SpecialProductBonusRule.TryGetBonusOf(harvestBox.ProductEntry.ProductName,
                    out specialBonusPrice);
            }
            harvestBox.Init(blinkDuration, isCropUnlocked, minimum, maximum, specialBonusPrice);
        }
    }

    public (int, int) GetPriceOf(string productName)
    {
        var harvestBox = _harvestBoxes.Values.FirstOrDefault(box => box.IsAvailable && box.ProductEntry.ProductName == productName);
        return harvestBox is null ? (0, 0) : harvestBox.Price;
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
    /// <param name="earnGold"></param>
    /// <param name="playScreenHarvestAnimation"></param>
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
        var harvestBox = _harvestBoxes.FirstOrDefault(pair => pair.Key.ProductName == productName).Value;
        StartCoroutine(CoSellProduct(harvestBox, cropWorldPosition, count, currentMapId, currentStageId, earnGold, playScreenHarvestAnimation, goldEarnEffect));
    }

    public void RaisePriceOfNeverFilledQuotas()
    {
        foreach (var harvestBox in _harvestBoxes.Values.Where(box => box.IsAvailable))
        {
            harvestBox.RaisePriceIfNeverFilled();
        }
    }
    
    /// <summary>
    /// 낮 시작(0.0초) 에 호출될 것으로 기대되는 메소드.
    /// </summary>
    public void ResetAllQuotas()
    {
        foreach (var harvestBox in _harvestBoxes.Values.Where(box => box.IsAvailable))
        {
            harvestBox.ResetQuota();
        }
        
        _hotSpecialTurn = specialProductTurnInterval - 1; // 핫 스페셜 배정 시 0부터 시작하도록
        SelectHotSpecialIfNeeded();
    }

    private void SelectHotSpecialIfNeeded()
    {
        if (_harvestBoxes.Values.Where(box => box.IsAvailable).Any(box => box.HotSpacialState != HotSpacialState.None))
        {
            return;
        }
        
        _hotSpecialTurn = (_hotSpecialTurn + 1) % specialProductTurnInterval;
        var isSpecialTurn = _hotSpecialTurn == specialProductTurnInterval - 1;
        
        var quotaSum = _harvestBoxes.Values.Sum(box => box.Quota);
        var random = Random.Range(0, quotaSum);
        
        foreach (var box in _harvestBoxes.Values.Where(box => box.IsAvailable))
        {
            random -= box.Quota;
            if (random >= 0)
            {
                continue;
            }
        
            box.HotSpacialState = isSpecialTurn ? HotSpacialState.Special : HotSpacialState.Hot;
            break;
        }
    }

    private void ResetCyclesIfNeeded()
    {
        if (_harvestBoxes.Values.Where(box => box.IsAvailable).Any(box => box.Cycle == 0))
        {
            return;
        }
        

        foreach (var harvestBox in _harvestBoxes.Values.Where(box => box.IsAvailable))
        {
            harvestBox.ResetCycle();
        }
    }

    private void Awake()
    {
        _harvestBoxes = new Dictionary<ProductEntry, HarvestBox>();
        _canvas = transform.Find("Canvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        var boxAreaObject = transform.Find("Canvas/Drawer/BoxArea").gameObject;
        for (var childIndex = 0; childIndex < boxAreaObject.transform.childCount; ++childIndex)
        {
            var child = boxAreaObject.transform.GetChild(childIndex).gameObject;
            var harvestBox = child.GetComponent<HarvestBox>();
            _harvestBoxes.Add(harvestBox.ProductEntry, harvestBox);
        }
    }

    private IEnumerator CoSellProduct(HarvestBox harvestBox, Vector2 cropWorldPosition, int count, int currentMapId, int currentStageId, Action<int> earnGold, Action<ProductEntry, float, Vector2, Vector2> playScreenHarvestAnimation, GoldEarnEffect goldEarnEffect)
    {
        var interval = harvestBulkAnimationTime / count * 0.5f;
        var cropWorldPosition3 = new Vector3(cropWorldPosition.x, cropWorldPosition.y, 90.0f);
        Vector2 cropScreenPosition = Camera.main.WorldToScreenPoint(cropWorldPosition3);
        Vector2 harvestBoxScreenPosition = _canvas.worldCamera.WorldToScreenPoint(harvestBox.transform.position);

        goldEarnEffect.PlayEffect(0);
        while (count > 0)
        {
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

            harvestBox.FillQuota(out var price, out var reAssigned);
            if (reAssigned)
            {
                SelectHotSpecialIfNeeded();
                ResetCyclesIfNeeded();
            }
            count -= 1;

            earnGold(price);
            goldEarnEffect.transform.position = cropWorldPosition;
            goldEarnEffect.GoldDisplaying += price;

            yield return new WaitForSeconds(interval);
        }
    }
}
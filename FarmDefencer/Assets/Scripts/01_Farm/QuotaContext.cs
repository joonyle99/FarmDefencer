using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public sealed class QuotaContext : MonoBehaviour, IFarmSerializable
{
    public const int SpecialProductTurnInterval = 4;
    [Serializable]
    public class QuotaRules
    {
        [SerializeField] private MapEntry map;
        public MapEntry Map => map;
        
        [SerializeField] private QuotaFillingRule quotaFillingRule;
        public QuotaFillingRule QuotaFillingRule => quotaFillingRule;

        [SerializeField] private SpecialProductBonusRule specialProductBonusRule;
        public SpecialProductBonusRule SpecialProductBonusRule => specialProductBonusRule;
    }
    
    public event Action QuotaContextUpdated;
    
    [CanBeNull] public ProductEntry SpecialProduct { get; private set; }
    [CanBeNull] public ProductEntry HotProduct { get; private set; }
    
    [SerializeField] private QuotaRules[] quotaRules;
    
    private Dictionary<ProductEntry, int> _remainingQuotas; // 키가 없으면 잠긴 작물
    
    private Func<string, ProductEntry> _getProductEntry;

    private int _turn;
    
    public bool IsAllQuotaFilled => _remainingQuotas.Values.All(v => v <= 0);

    public void Init(Func<string, ProductEntry> getProductEntry) => _getProductEntry = getProductEntry;

    public JObject Serialize()
    {
        var json = JObject.FromObject(_remainingQuotas.ToDictionary(kv => kv.Key.ProductName, kv => kv.Value));
        json.Add(new JProperty("Turn", _turn));
        if (HotProduct is not null)
        {
            json.Add(new JProperty("HotProduct", HotProduct.ProductName));
        }

        if (SpecialProduct is not null)
        {
            json.Add(new JProperty("SpecialProduct", SpecialProduct.ProductName));
        }

        return json;
    }

    public void Deserialize(JObject json)
    {
        _remainingQuotas.Clear();

        var dictionary = json
            .Properties()
            .Where(p => p.Value.Type == JTokenType.Integer && !p.Name.Equals("Turn"))
            .ToDictionary(p => p.Name, p => p.Value.Value<int>());

        foreach (var (productName, quota) in dictionary)
        {
            var entry = _getProductEntry(productName);
            if (entry is null)
            {
                Debug.LogError($"{productName}에 해당하는 ProductEntry를 찾지 못했습니다.");
                continue;
            }
            _remainingQuotas.Add(entry, quota);
        }
        
        _turn = json.Property("Turn")?.Value.Value<int>() ?? 0;
        HotProduct = json.Property("HotProduct")?.Value.Value<string>() is string hotProductName ? _getProductEntry(hotProductName) : null;
        SpecialProduct = json.Property("SpecialProduct")?.Value.Value<string>() is string specialProductName ? _getProductEntry(specialProductName) : null;
        
        QuotaContextUpdated?.Invoke();
    }

    public bool IsProductAvailable(ProductEntry product) => _remainingQuotas.ContainsKey(product);

    public bool TryGetQuota(string productName, out int quota)
    {
        var found = _remainingQuotas.FirstOrDefault(q => q.Key.ProductName.Equals(productName));
        quota = found.Key is null ? 0 : found.Value;
        return found.Key is not null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="quota"></param>
    /// <returns>Hot, Special 작물을 고려한 가격.</returns>
    public int FillQuota(string productName, int quota, int currentMapId)
    {
        var (productEntry, remainingQuota) = _remainingQuotas.FirstOrDefault(q => q.Key.ProductName.Equals(productName));
        if (productEntry is null)
        {
            Debug.LogError($"{productName} 에 해당하는 주문량 정보를 찾지 못하였습니다.");
            return 0;
        }
        
        if (quota > remainingQuota)
        {
            Debug.LogWarning($"{productName} 의 남은 주문량 {remainingQuota} 보다 많은 {quota} 만큼의 주문량을 채우려고 시도했습니다.");
            quota = remainingQuota;
        }
        
        var totalPrice = productEntry.Price * quota;
        if (productEntry == HotProduct)
        {
            totalPrice *= 2;
        }
        else if (productEntry == SpecialProduct && _remainingQuotas[productEntry] == quota)
        {
            var ruleForMap = quotaRules.FirstOrDefault(r => r.Map.MapId == currentMapId);
            if (ruleForMap is not null && ruleForMap.SpecialProductBonusRule.TryGetBonusOf(productName, out var bonusPrice))
            {
                totalPrice += bonusPrice;
            }
            else
            {
                Debug.LogError($"{currentMapId} 에 해당하는 Special Product Bonus Rule을 찾을 수 없습니다.");
            }
        }
        
        _remainingQuotas[productEntry] -= quota;
        QuotaContextUpdated?.Invoke();

        return totalPrice;
    }

    public void AssignQuotas(int currentMapId)
    {
        _remainingQuotas.Clear();
        
        _turn = (_turn + 1) % SpecialProductTurnInterval;
        var isSpecial = _turn == SpecialProductTurnInterval - 1;

        var ruleForMap = quotaRules.FirstOrDefault(r => r.Map.MapId == currentMapId);
        if (ruleForMap is null)
        {
            Debug.LogError($"{currentMapId} 에 해당하는 QuotaFillingRuleForMap을 찾을 수 없습니다.");
            return;
        }

        var quotaSum = 0;
        foreach (var ruleEntry in ruleForMap.QuotaFillingRule.Entries)
        {
            var (minimum, maximum) = ruleEntry.Range;
            if (maximum < minimum)
            {
                Debug.LogError($"{ruleEntry.Crop.ProductName}에 해당하는 QuotaFillingRuleEntry의 개수 범위가 잘못되었습니다.");
                continue;
            }

            var quota = Random.Range(minimum, maximum + 1);
            _remainingQuotas[ruleEntry.Crop] = quota;
            quotaSum += quota;
        }
        
        var random = Random.Range(0, quotaSum);
        foreach (var (productEntry, quota) in _remainingQuotas)
        {
            random -= quota;
            if (random < 0)
            {
                SpecialProduct = isSpecial ? productEntry : null;
                HotProduct = !isSpecial ? productEntry : null;
                break;
            }
        }
        
        QuotaContextUpdated?.Invoke();
    }
    
    private void Awake()
    {
        _remainingQuotas = new();
    }
}

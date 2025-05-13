using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class QuotaContext : MonoBehaviour, IFarmSerializable
{
    [Serializable]
    public class QuotaFillingRuleForMap
    {
        [SerializeField] private MapEntry map;
        public MapEntry Map => map;
        
        [SerializeField] private QuotaFillingRule rule;
        public QuotaFillingRule Rule => rule;
    }
    
    public event Action QuotaContextUpdated;
    
    [SerializeField] private QuotaFillingRuleForMap[] quotaFillingRules;
    
    private Dictionary<ProductEntry, int> _remainingQuotas; // 키가 없으면 잠긴 작물
    
    private Func<string, ProductEntry> _getProductEntry;

    public bool IsAllQuotaFilled => _remainingQuotas.Values.All(v => v <= 0);

    public void Init(Func<string, ProductEntry> getProductEntry) => _getProductEntry = getProductEntry;

    public JObject Serialize() => JObject.FromObject(_remainingQuotas.ToDictionary(kv => kv.Key.ProductName, kv => kv.Value));

        public void Deserialize(JObject json)
        {
            _remainingQuotas.Clear();

            var dictionary = json
                .Properties()
                .Where(p => p.Value.Type == JTokenType.Integer)
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
                Debug.Log($"{entry.ProductName}, {quota}");
            }
            
            QuotaContextUpdated?.Invoke();
        }

    public bool IsProductAvailable(ProductEntry product) => _remainingQuotas.ContainsKey(product);

    public bool TryGetQuota(string productName, out int quota)
    {
        var found = _remainingQuotas.FirstOrDefault(q => q.Key.ProductName.Equals(productName));
        quota = found.Key is null ? 0 : found.Value;
        return found.Key is not null;
    }

    public void FillQuota(string productName, int quota)
    {
        var (productEntry, remainingQuota) = _remainingQuotas.FirstOrDefault(q => q.Key.ProductName.Equals(productName));
        if (productEntry is null)
        {
            Debug.LogError($"{productName} 에 해당하는 주문량 정보를 찾지 못하였습니다.");
            return;
        }
        
        if (quota > remainingQuota)
        {
            Debug.LogWarning($"{productName} 의 남은 주문량 {remainingQuota} 보다 많은 {quota} 만큼의 주문량을 채우려고 시도했습니다.");
            quota = remainingQuota;
        }

        _remainingQuotas[productEntry] -= quota;
        QuotaContextUpdated?.Invoke();
    }

    public void AssignQuotas(int currentMapId)
    {
        _remainingQuotas.Clear();

        var ruleForMap = quotaFillingRules.FirstOrDefault(r => r.Map.MapId == currentMapId);
        if (ruleForMap is null)
        {
            Debug.LogError($"{currentMapId} 에 해당하는 QuotaFillingRuleForMap을 찾을 수 없습니다.");
            return;
        }
        
        foreach (var ruleEntry in ruleForMap.Rule.Entries)
        {
            var (minimum, maximum) = ruleEntry.Range;
            if (maximum < minimum)
            {
                Debug.LogError($"{ruleEntry.Crop.ProductName}에 해당하는 QuotaFillingRuleEntry의 개수 범위가 잘못되었습니다.");
                continue;
            }

            var quota = Random.Range(minimum, maximum + 1);
            _remainingQuotas[ruleEntry.Crop] = quota;
        }
        
        QuotaContextUpdated?.Invoke();
    }
    
    private void Awake()
    {
        _remainingQuotas = new();
    }
}

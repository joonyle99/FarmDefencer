using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "SpecialProductBonusRule", menuName = "Scriptable Objects/Farm/Special Product Bonus Rule")]
public sealed class SpecialProductBonusRule : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [SerializeField] private ProductEntry productEntry;
        [SerializeField] private int bonusPrice;

        public ProductEntry ProductEntry => productEntry;
        public int BonusPrice => bonusPrice;
    }

    [SerializeField] private List<Entry> rule;

    public bool TryGetBonusOf(string productName, out int price)
    {
        var entry = rule.FirstOrDefault(e => e.ProductEntry.ProductName.Equals(productName));
        price = entry?.BonusPrice ?? 0;
        return entry is not null;
    }
}

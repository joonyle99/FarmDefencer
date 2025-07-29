using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuotaFillingRule", menuName = "Scriptable Objects/Farm/Quota Filling Rule")]
public sealed class QuotaFillingRule : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        [Header("어느 작물을 대상으로?")] [SerializeField] private ProductEntry targetProduct;
        public ProductEntry TargetProduct => targetProduct;
        
        [Header("최소 몇개?")] [SerializeField] private int minimumCount;
        [Header("최대 몇개?")] [SerializeField] private int maximumCount;

        public (int, int) Range => (minimumCount, maximumCount);
    }

    
    [Serializable]
    public class PerMapEntries
    {
        [Header("어느 맵에 적용?")][SerializeField] private MapEntry targetMap;
        public MapEntry TargetMap => targetMap;

        [Header("이 맵에 적용할 작물별 규칙들")][SerializeField] private Entry[] entries;
        public IReadOnlyList<Entry> Entries => entries;

        [Header("이 맵에 적용할 스페셜 작물 보너스 규칙")][SerializeField] private SpecialProductBonusRule specialProductBonusRule;
        public SpecialProductBonusRule SpecialProductBonusRule => specialProductBonusRule;
    }

    [Header("규칙 적용할 맵들")][SerializeField] private PerMapEntries[] perMapEntries;
    public PerMapEntries[] Entries => perMapEntries;
}

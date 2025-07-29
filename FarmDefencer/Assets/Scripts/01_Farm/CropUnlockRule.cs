using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 작물이 언제 해금되는지를 결정하는 클래스.
/// 유연성을 고려하여 ProductEntry나 QuotaFillingRule에 포함시키지 않고 분리함.
/// </summary>
[CreateAssetMenu(fileName = "CropUnlockRule", menuName = "Scriptable Objects/Farm/Crop Unlock Rule")]
public sealed class CropUnlockRule : ScriptableObject
{
    [Serializable]
    public struct CropUnlockRuleEntry
    {
        [SerializeField] private ProductEntry targetCrop;
        public ProductEntry TargetCrop => targetCrop;
        [SerializeField] private int minMapIndex;
        public int MinMapIndex => minMapIndex;
        [SerializeField] private int minStageIndex;
        public int MinStageIndex => minStageIndex;
    }

    [SerializeField] private CropUnlockRuleEntry[] entries;
    public IReadOnlyList<CropUnlockRuleEntry> Entries => entries;

    public bool IsCropUnlocked(string productName, int mapIndex, int stageIndex) => entries.Any(e =>
        e.TargetCrop.ProductName.Equals(productName) && (mapIndex > e.MinMapIndex || mapIndex == e.MinMapIndex && stageIndex >= e.MinStageIndex));
}

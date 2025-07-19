using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PestSpawnRule", menuName = "Scriptable Objects/Farm/Pest Spawn Rule")]
public sealed class PestSpawnRule : ScriptableObject
{
    [Serializable]
    public class PestSpawnRuleEntry
    {
        [Header("기준 해금 작물")][SerializeField] private ProductEntry preconditionEnabledProduct;
        public ProductEntry PreconditionEnabledProduct => preconditionEnabledProduct;
        
        [Header("왼쪽에서 스폰되는 해충 규칙")]
        [SerializeField] private int leftSmallCount;
        [SerializeField] private int leftMediumCount;
        [SerializeField] private int leftLargeCount;
        
        [Header("오른쪽에서 스폰되는 해충 규칙")]
        [SerializeField] private int rightSmallCount;
        [SerializeField] private int rightMediumCount;
        [SerializeField] private int rightLargeCount;
        
        // public int GetCountOf(PestOrigin pestOrigin, PestSize pestSize)
        // {
        //     var isLeft = pestOrigin == PestOrigin.Left;
        //     return pestSize switch
        //     {
        //         PestSize.Small => isLeft ? leftSmallCount : rightSmallCount,
        //         PestSize.Medium => isLeft ? leftMediumCount : rightMediumCount,
        //         _ => isLeft ? leftLargeCount : rightLargeCount
        //     };
        // }

        public List<(PestOrigin, PestSize)> ToList()
        {
            var list = new List<(PestOrigin, PestSize)>();
            for (int i = 0; i < leftSmallCount; i++) list.Add(new(PestOrigin.Left, PestSize.Small));
            for (int i = 0; i < leftMediumCount; i++) list.Add(new(PestOrigin.Left, PestSize.Medium));
            for (int i = 0; i < leftLargeCount; i++) list.Add(new(PestOrigin.Left, PestSize.Large));
            for (int i = 0; i < rightSmallCount; i++) list.Add(new(PestOrigin.Right, PestSize.Small));
            for (int i = 0; i < rightMediumCount; i++) list.Add(new(PestOrigin.Right, PestSize.Medium));
            for (int i = 0; i < rightLargeCount; i++) list.Add(new(PestOrigin.Right, PestSize.Large));
            return list;
        }
    }
    
    [FormerlySerializedAs("spawnRules")]
    [Header("작물 열리는 순서대로 작성")]
    [SerializeField] private List<PestSpawnRuleEntry> rule;
    public IReadOnlyList<PestSpawnRuleEntry> Rule => rule;
}

using UnityEngine;

[CreateAssetMenu(fileName = "QuotaFillingRule", menuName = "Scriptable Objects/Farm/Quota Filling Rule")]
public sealed class QuotaFillingRule : ScriptableObject
{
    [System.Serializable]
    public sealed class QuotaFillingRuleEntry
    {
        [SerializeField] private ProductEntry crop;
        public ProductEntry Crop => crop;
        
        [SerializeField] private int minimumCount;
        [SerializeField] private int maximumCount;

        public (int, int) Range => (minimumCount, maximumCount);
    }
    
    [SerializeField] private QuotaFillingRuleEntry[] entries;
    public QuotaFillingRuleEntry[] Entries => entries;
}

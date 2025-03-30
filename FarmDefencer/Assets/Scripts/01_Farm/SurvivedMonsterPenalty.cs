using UnityEngine;

[System.Serializable]
public sealed class CropProbabilityData
{
    [SerializeField] private ProductEntry targetCrop;
    [SerializeField] private float probability;

    public ProductEntry TargetCrop => targetCrop;
    public float Probability => probability;
}

[CreateAssetMenu(fileName = "SurvivedMonsterPenalty", menuName = "Scriptable Objects/Farm/Survived Monster Penalty")]
public sealed class SurvivedMonsterPenalty : ScriptableObject
{
    [System.Serializable]
    public class MonsterPenaltyData
    {
        [SerializeField] private string monster;
        [SerializeField] private CropProbabilityData[] cropProbabilities;

        public string Monster => monster;
        public CropProbabilityData[] CropProbabilityDatas => cropProbabilities;
    }

    [SerializeField] private MonsterPenaltyData[] monsterPenaltyDatas;

    public MonsterPenaltyData[] MonsterPenaltyDatas => monsterPenaltyDatas;
}

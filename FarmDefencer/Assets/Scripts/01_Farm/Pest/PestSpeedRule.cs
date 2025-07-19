using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "PestSpeedRule", menuName = "Scriptable Objects/Farm/Pest Speed Rule")]
public sealed class PestSpeedRule : ScriptableObject
{
    [Serializable]
    public class PestSpeedRuleEntry
    {
        [Header("이 규칙을 적용할 맵")] [SerializeField] private MapEntry preconditionCurrentMap;
        public MapEntry PreconditionCurrentMap => preconditionCurrentMap;

        [Header("작은놈")] 
        [SerializeField] private float smallPestMinimumSpeed = 10.0f;
        [SerializeField] private float smallPestMaximumSpeed = 10.0f;
        public Vector2 SmallPestSpeedRange => new Vector2(smallPestMinimumSpeed, smallPestMaximumSpeed);
        
        [Header("중간놈")] 
        [SerializeField] private float mediumPestMinimumSpeed = 10.0f;
        [SerializeField] private float mediumPestMaximumSpeed = 10.0f;     
        public Vector2 MediumPestSpeedRange => new Vector2(mediumPestMinimumSpeed, mediumPestMaximumSpeed);
        
        [Header("큰놈")] 
        [SerializeField] private float largePestMinimumSpeed = 10.0f;
        [SerializeField] private float largePestMaximumSpeed = 10.0f;
        public Vector2 LargePestSpeedRange => new Vector2(largePestMinimumSpeed, largePestMaximumSpeed);
    }
    
    [SerializeField] private List<PestSpeedRuleEntry> pestSpeedRuleEntries;
    public IReadOnlyList<PestSpeedRuleEntry> Rule => pestSpeedRuleEntries;
}

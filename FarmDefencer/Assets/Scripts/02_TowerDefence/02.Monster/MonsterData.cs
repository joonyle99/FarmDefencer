using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/TowerDefence/MonsterData", order = 11)]
public class MonsterData : ScriptableObject
{
    [BoxGroup("기본 정보")] public string Name;
    [BoxGroup("기본 정보")] public string Description;

    [FoldoutGroup("능력치 - 속도")] public int MoveSpeed;
}

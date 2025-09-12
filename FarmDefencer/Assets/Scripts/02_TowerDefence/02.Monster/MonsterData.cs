using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/TowerDefence/MonsterData", order = 11)]
public class MonsterData : ScriptableObject
{
    [BoxGroup("기본 정보")] public string Name;
    [BoxGroup("기본 정보")] public string Description;

    [FoldoutGroup("능력치 - 체력")] public int MaxHp = 100;

    /// <summary>
    /// 몬스터가 이동하는 속도입니다.
    /// 기본 값은 1입니다. (1초에 1칸 이동)
    /// 값이 2인 경우 -> 1초에 2칸 이동합니다.
    /// 값이 0.5인 경우 -> 1초에 0.5칸 이동합니다.
    /// </summary>
    [FoldoutGroup("능력치 - 속도")] public float OriginMoveSpeed = 1f;
}

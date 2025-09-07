using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerDefence/TowerLevelData", order = 10)]
public class TowerLevelData : ScriptableObject
{
    [BoxGroup("외형")] public Sprite Icon;

    [BoxGroup("기본 정보")] public string Name;
    [BoxGroup("기본 정보")] public string Description;
    [BoxGroup("기본 정보")] public int Level;

    [FoldoutGroup("능력치 - 기본")] public int AttackDamage;
    [FoldoutGroup("능력치 - 기본")] public float AttackRate;
    [FoldoutGroup("능력치 - 슬로우")] public float SlowRate;
    [FoldoutGroup("능력치 - 슬로우")] public float SlowDuration;

    [BoxGroup("비용")] public int ValueCost;
    [BoxGroup("비용")] public int SellCost;

    [BoxGroup("사운드")] public AudioClip FireReady;
    [BoxGroup("사운드")] public AudioClip FireShot;

    [BoxGroup("기타 - 타워 2")] public int TickCount;
    [BoxGroup("기타 - 타워 2")] public float TickInterval;
    [BoxGroup("기타 - 타워 2")] public int TickDamage;

    [BoxGroup("기타 - 타워 4")] public float StayDuration;
    [BoxGroup("기타 - 타워 4")] public float DealInterval;

    [BoxGroup("기타 - 타워 5")] public int MaxTransitionCount;
}

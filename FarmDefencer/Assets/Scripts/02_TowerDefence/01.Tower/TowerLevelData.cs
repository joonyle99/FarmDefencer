using UnityEngine;

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerDefence/TowerLevelData")]
public class TowerLevelData : ScriptableObject
{
    [Header("1. 외형")]
    public Sprite Icon;

    [Header("1. 외형")]
    public string Name;
    public string Description;
    public int Level;

    [Header("3. 능력치")]
    public float AttackRate;
    public float SlowRate;
    public float SlowDuration;
    public int Damage;

    [Header("4. 비용")]
    public int ValueCost;
    public int SellCost;

    [Header("5. 사운드")]
    public AudioClip FireReady;
    public AudioClip FireShot;
}

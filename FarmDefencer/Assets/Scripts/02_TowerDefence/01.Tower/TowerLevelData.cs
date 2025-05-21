using UnityEngine;

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerDefence/TowerLevelData")]
public class TowerLevelData : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public string Description;
    public int Level;
    public float AttackRate;
    public float SlowRate;
    public float SlowDuration;
    public int Damage;
    public int ValueCost;
    public int SellCost;
}

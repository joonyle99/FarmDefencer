using UnityEngine;

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerDefence/TowerLevelData")]
public class TowerLevelData : ScriptableObject
{
    public int Level;
    public float AttackRate;
    public int Damage;
    public int ValueCost;
    public int SellCost;
}

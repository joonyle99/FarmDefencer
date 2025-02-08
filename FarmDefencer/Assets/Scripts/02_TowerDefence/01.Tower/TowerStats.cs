using TMPro;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    public Tower tower;

    public TextMeshPro levelText;
    public TextMeshPro attackRateText;
    public TextMeshPro damageText;
    public TextMeshPro costText;

    private void OnEnable()
    {
        SetStats(tower.CurrentLevel, tower.CurrentAttackRate, tower.CurrentDamage, tower.Cost);

        tower.OnLevelChanged -= SetLevelText;
        tower.OnLevelChanged += SetLevelText;

        tower.OnAttackRateChanged -= SetAttackRateText;
        tower.OnAttackRateChanged += SetAttackRateText;

        tower.OnDamageChanged -= SetDamageText;
        tower.OnDamageChanged += SetDamageText;

        tower.OnCostChanged -= SetCostText;
        tower.OnCostChanged += SetCostText;
    }
    private void OnDisable()
    {
        tower.OnLevelChanged -= SetLevelText;
        tower.OnAttackRateChanged -= SetAttackRateText;
        tower.OnDamageChanged -= SetDamageText;
        tower.OnCostChanged -= SetCostText;
    }

    private void SetStats(int level, float attackRate, int damage, int cost)
    {
        SetLevelText(level);
        SetAttackRateText(attackRate);
        SetDamageText(damage);
        SetCostText(cost);
    }

    private void SetLevelText(int level)
    {
        levelText.text = $"level: {level}";
    }
    private void SetAttackRateText(float attackRate)
    {
        attackRateText.text = $"attackRate: {attackRate}";
    }
    private void SetDamageText(int damage)
    {
        damageText.text = $"damage: {damage}";
    }
    private void SetCostText(int cost)
    {
        costText.text = $"cost: {cost}";
    }
}

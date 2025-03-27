using System.Collections;
using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("──────── Damager ────────")]
    [Space]

    [SerializeField] private int _damage;

    public int GetDamage()
    {
        return _damage;
    }
    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    public void DealDamage(DamageableBehavior damageable)
    {
        damageable.TakeDamage(_damage);
    }
    public void DealTickDamage(DamageableBehavior damageable, int tickCount, float tickInterval, int tickDamage)
    {
        damageable.TakeTickDamage(tickCount, tickInterval, tickDamage);
    }
}

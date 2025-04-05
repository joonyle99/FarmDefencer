using System.Collections;
using UnityEngine;

public enum DamageType
{
    Normal,
    Fire,
    Poison,
    Electric,
}

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
    public void DealDamage(DamageableBehavior damageable, DamageType type)
    {
        damageable.TakeDamage(_damage, type);
    }
    public void DealTickDamage(DamageableBehavior damageable, int count, float interval, int damage, DamageType type)
    {
        damageable.TakeTickDamage(count, interval, damage, type);
    }
}

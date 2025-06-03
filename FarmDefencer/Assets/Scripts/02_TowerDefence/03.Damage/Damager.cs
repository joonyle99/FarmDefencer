using UnityEngine;

public enum ProjectileType
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
    public void DealDamage(DamageableBehavior damageable, ProjectileType type)
    {
        damageable.TakeDamage(_damage, type);
    }
    public void DealTickDamage(DamageableBehavior damageable, int count, float interval, int damage, ProjectileType type)
    {
        damageable.TakeTickDamage(count, interval, damage, type);
    }
}

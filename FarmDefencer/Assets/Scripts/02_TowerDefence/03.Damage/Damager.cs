using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("──────── Damager ────────")]
    [Space]

    [SerializeField] private int _damage = 10;

    public int GetDamage()
    {
        return _damage;
    }
    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    public void HasDamaged(DamageableBehavior damageable)
    {
        damageable.TakeDamage(_damage);
    }
}

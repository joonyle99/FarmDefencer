using UnityEngine;

public class Damager : MonoBehaviour
{
    private float _damage;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
    public void HasDamaged(DamageableBehavior damageable)
    {
        damageable.TakeDamage(_damage);
    }
}

using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("式式式式式式式式 Damager 式式式式式式式式")]
    [Space]

    [SerializeField] private float _damage = 10;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
    public void HasDamaged(DamageableBehavior damageable)
    {
        damageable.TakeDamage(_damage);
    }
}

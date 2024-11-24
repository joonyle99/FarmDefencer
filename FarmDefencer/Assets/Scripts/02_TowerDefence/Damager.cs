using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("���������������� Damager ����������������")]
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

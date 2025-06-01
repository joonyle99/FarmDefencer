using UnityEngine;

/// <summary>
/// DamageableCollider은 Collider 기반으로 Damager와 DamageableBehavior를 연결해주는 역할을 한다.
/// </summary>
public sealed class DamageableCollider : DamageableZone
{
    protected override void Awake()
    {
        base.Awake();

        if (damageCollider.isTrigger == true)
        {
            Debug.LogError("Turn off the \'isTrigger\' checkbox");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damager = collision.gameObject.GetComponent<Damager>();

        if (damager == null)
        {
            return;
        }

        // Debug.Log($"onCollisionEnter - {this.gameObject.name}");

        damager.DealDamage(demageableBehavior, ProjectileType.Normal);
        Destroy(damager.gameObject);
    }
}

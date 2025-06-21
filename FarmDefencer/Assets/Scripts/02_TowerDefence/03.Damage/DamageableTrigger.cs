using UnityEngine;

public sealed class DamageableTrigger : DamageableZone
{
    protected override void Awake()
    {
        base.Awake();

        if (damageCollider.isTrigger == false)
        {
            Debug.LogError("Turn on the \'isTrigger\' checkbox");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damager = collision.gameObject.GetComponent<Damager>();

        if (damager == null)
        {
            return;
        }

        // Debug.Log($"onTriggerEnter - {this.gameObject.name}");

        damager.DealDamage(demageableBehavior, DamageType.Normal);
        Destroy(damager.gameObject);
    }
}

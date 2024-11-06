using UnityEngine;

public class DamageCollider : DamageZone
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        var damager = collision.gameObject.GetComponent<Damager>();
        if(damager == null)
        {
            return;
        }
        damager.HasDamaged(demageableBehavior);
        */
    }

    private void OnCollisionExit2D(Collision2D collision)
    {

    }

    private void OnCollisionStay2D(Collision2D collision)
    {

    }
}

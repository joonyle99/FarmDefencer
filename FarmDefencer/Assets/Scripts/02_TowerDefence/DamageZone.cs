using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public abstract class DamageZone : MonoBehaviour
{
    protected DamageableBehavior demageableBehavior;        // to
    protected Damager damager;                              // from


}

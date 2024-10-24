using UnityEngine;

public abstract class Target : MonoBehaviour, ITargetable
{
    public abstract void Hurt();
}

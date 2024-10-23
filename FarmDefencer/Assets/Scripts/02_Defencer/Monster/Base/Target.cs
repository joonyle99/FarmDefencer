using UnityEngine;

public interface ITargetable
{
    public void Hit();
}

public abstract class Target : MonoBehaviour, ITargetable
{
    // 공통 변수

    // 공통 함수

    public abstract void Hit();
}

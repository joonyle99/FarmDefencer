using UnityEngine;

/// <summary>
/// 타워의 공격을 받을 수 있는 대상.
/// 몬스터 뿐만 아니라, ???도 타워의 공격을 받을 수 있습니다
/// </summary>
public abstract class Target : MonoBehaviour, ITargetable
{
    public abstract void Hurt();
}

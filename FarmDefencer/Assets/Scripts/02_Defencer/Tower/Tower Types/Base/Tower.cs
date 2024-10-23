using System.Linq;
using UnityEngine;

public interface IAttackable
{
    public void Attack();
}

public abstract class Tower : MonoBehaviour, IAttackable
{
    private Transform _firePoint;
    private TargetDetector _targetDetector;
    private ITargetingStrategy _targetingStrategy;

    public void SetTargetingStrategy(ITargetingStrategy targetingStrategy)
    {
        _targetingStrategy = targetingStrategy;
    }

    // 언제 공격을 할것인가?
    public void Attack()
    {
        var allTargets = _targetDetector.Targets.ToArray<Target>();

        /*
        var selectedTargets = _targetingStrategy.SelectTargets(allTargets);

        if (selectedTargets == null)
        {
            Debug.Log("There are no selectedTargets");
            return;
        }
        */

        foreach (var target in allTargets)
        {
            target.Hit();
        }
    }
}

/// 고려해야 하는 속성들

// 타워의 체력
// 타워의 방어력
// 타워의 공격력
// 타워의 공격속도
// 타워의 공격 효과 (슬로우, 중독, 기절)
// 타워의 사정거리 범위
// 타워의 타게팅 가능 수
// 타워의 공격 대상 (지상, 공중, 수중)
// 타워의 타게팅 방식 (가장 가까운 적, 체력이 적은 적, 혹은 가장 빠른 적)

// 타워 설치 비용
// 타워 설치 쿨타임

// 타워 업그레이드 시스템?
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 대상을 타격하면 인접한 적들에게 번개가 연쇄적으로 전달됩니다.
/// </summary>
public sealed class LightningProjectile : LinearProjectile
{
    [Header("──────── Lightning Projectile ────────")]
    [Space]

    public int MaxCount = 3;

    protected override void DealDamage()
    {
        // damager.DealDamage(target, DamageType.Normal);
    }
    protected override void DealEffect()
    {
        // 모든 타겟을 미리 정해서 반환한다
        var targets = FindTargetsWithDFS(target, MaxCount);

        // ChainLightning (체인 라이트닝) 생성해 LightningEffector에 전달
        // 이 때 체인 라이트닝 게임 오브젝트는 씬의 루트에 생성
        // caster만 초기 설정하고 이후에 lightning effector에서 target을 추가하는 방식
        var chainLightningPrefab = Resources.Load<ChainLightning>("Prefabs/ChainLightning");
        var chainLightning = Instantiate(chainLightningPrefab, Vector3.zero, Quaternion.identity);
        chainLightning.Initialize(caster, targets, damager.GetDamage());
    }

    public List<TargetableBehavior> FindTargetsWithDFS(TargetableBehavior startTarget, int maxDepth)
    {
        var targets = new List<TargetableBehavior>();
        var visited = new HashSet<TargetableBehavior>();

        // 첫 타겟 추가
        targets.Add(startTarget);
        visited.Add(startTarget);

        void FindNextTarget(TargetableBehavior currTarget, int depth)
        {
            // DFS 종료 조건
            if (depth >= maxDepth)
            {
                return;
            }

            var originCell = currTarget.GridMovement.CurrGridCell;
            foreach (var dir in ConstantConfig.DirectionsWithOrigin)
            {
                var posX = originCell.cellPosition.x + dir.x;
                var posY = originCell.cellPosition.y + dir.y;
                var cell = DefenceContext.Current.GridMap.GetCell(posX, posY);
                if (cell == null)
                {
                    // 유효하지 않은 그리드 셀에 대해서는 처리하지 않는다
                    continue;
                }

                // 여러 몬스터가 있을 경우 랜덤하게 하나 선택
                var candidates = cell.monstersInCell.Where(monster => monster != null && monster.gameObject.activeInHierarchy && !monster.IsDead && !visited.Contains(monster)).ToList();

                if (candidates.Count > 0)
                {
                    var nextTarget = candidates[Random.Range(0, candidates.Count)];

                    // 다음 타겟 추가
                    targets.Add(nextTarget);
                    visited.Add(nextTarget);

                    FindNextTarget(nextTarget, depth + 1);

                    // 만약 여러 갈래로 체인하고 싶으면 foreach로 모두 Find(next, depth+1) 호출
                    // 단일 체인만 원하면 break;

                    break;
                }
            }
        }

        FindNextTarget(startTarget, 1);

        return targets;
    }
}

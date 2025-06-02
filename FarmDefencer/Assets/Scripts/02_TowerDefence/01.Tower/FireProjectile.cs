using UnityEngine;
using DG.Tweening;

/// <summary>
/// 불덩이 투사체 탄환
/// 타격 시 화상 효과를 적용
/// </summary>
public sealed class FireProjectile : ParabolicProjectile
{
    [Header("──────── Fire Projectile ────────")]
    [Space]

    [SerializeField] private int _tickCount;
    [SerializeField] private float _tickInterval;
    [SerializeField] private int _tickDamage;

    private static readonly (int x, int y)[] DIRECTION = new (int x, int y)[]
    {
        (0, 0),    // origin
        (-1, 0),   // left
        (1, 0),    // right
        (0, -1),   // down
        (0, 1)     // up
    };

    protected override void DealDamage()
    {
        // 직접 데미지
        damager.DealDamage(currentTarget, ProjectileType.Fire);

        // 화상 효과
        DealThickDamage();
    }
    protected override void DealEffect()
    {
        // do nothing
    }

    private void DealThickDamage()
    {
        // 지속(Tick) 데미지
        var originCell = currentTarget.GridMovement.CurrGridCell;

        foreach (var dir in DIRECTION)
        {
            var cell = DefenceContext.Current.GridMap.GetCell(originCell.cellPosition.x + dir.x, originCell.cellPosition.y + dir.y);
            if (cell == null) continue;

            // 1) 지속 데미지 부여
            // currentTarget 뿐만 아니라 monstersInCell에 있는 모든 몬스터에 대해 유효성 검사를 진행해야 한다
            cell.monstersInCell.ForEach(monster =>
            {
                // monster == null: 일반적인 Unity-style null 검사 → Destroy된 객체 포함
                // monster.Equals(null): 혹시 모를 내부 에러 방지 및 명시적 비교 (C# 오브젝트)
                if (monster == null || monster.Equals(null)) return;
                if (!monster.gameObject.activeInHierarchy) return;

                damager.DealTickDamage(monster, _tickCount, _tickInterval, _tickDamage, ProjectileType.Fire);
            });

            // 2) 셀 시각 효과
            if (DOTween.IsTweening(cell.transform))
            {
                //DOTween.Kill(cell.transform);
                continue;
            }
            else
            {
                cell.transform?.DOScale(1.5f, 0.25f).SetEase(Ease.OutBack).OnComplete(() => { cell.transform.DOScale(1f, 0.25f).SetEase(Ease.InBack); });
            }
        }
    }
}

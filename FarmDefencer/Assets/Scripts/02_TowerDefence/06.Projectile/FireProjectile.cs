using DG.Tweening;
using UnityEngine;

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

    protected override void DealDamage()
    {
        damager.DealDamage(target, DamageType.Normal);
    }
    protected override void DealEffect()
    {
        // 범위 내 모든 적에게 화상 효과를 적용한다
        var originCell = target.GridMovement.CurrGridCell;
        foreach (var dir in ConstantConfig.DIRECTIONS)
        {
            var posX = originCell.cellPosition.x + dir.x;
            var posY = originCell.cellPosition.y + dir.y;
            var cell = DefenceContext.Current.GridMap.GetCell(posX, posY);
            if (cell == null)
            {
                // 유효하지 않은 그리드 셀에 대해서는 처리하지 않는다
                continue;
            }

            // 1) 지속 데미지 부여
            // currentTarget 뿐만 아니라 monstersInCell에 있는 모든 몬스터에 대해 유효성 검사를 진행해야 한다
            cell.monstersInCell.ForEach(monster =>
            {
                // monster == null: 일반적인 Unity-style null 검사 → Destroy된 객체 포함
                // monster.Equals(null): 혹시 모를 내부 에러 방지 및 명시적 비교 (C# 오브젝트)
                if (monster == null || monster.Equals(null)) return;
                if (!monster.gameObject.activeInHierarchy) return;

                // 화상 효과 적용 (중복 적용 가능)
                var burnEffector = monster.gameObject.AddComponent<BurnEffector>();
                burnEffector.Activate(monster, _tickCount, _tickInterval, _tickDamage);
            });

            // // 2) 셀 시각 효과
            // if (DOTween.IsTweening(cell.transform))
            // {
            //     //DOTween.Kill(cell.transform);
            //     continue;
            // }
            // else
            // {
            //     cell.transform.DOScale(1.5f, 0.25f).SetEase(Ease.OutBack).OnComplete(()
            //         =>
            //     { cell.transform.DOScale(1f, 0.25f).SetEase(Ease.InBack); });
            // }
        }
    }
}

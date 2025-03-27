using UnityEngine;
using DG.Tweening;

/// <summary>
/// 포물선 투사체 탄환
/// </summary>
public sealed class ParabolicProjectile : ProjectileBase
{
    [Header("──────── Parabolic Projectile ────────")]
    [Space]

    [SerializeField] private float _peekHeight = 1.5f;
    [SerializeField] private AnimationCurve _curve;

    protected override void Move()
    {
        // move
        float heightT = _curve.Evaluate(linearT);
        float height = Mathf.Lerp(0f, _peekHeight, heightT);
        Vector2 pos = Vector2.Lerp(startPos, currentTarget.transform.position, linearT) + Vector2.up * height;
        transform.position = pos;
    }
    protected override void Rotate()
    {
        // rotate
        float deltaT = 0.001f;
        float nextLinearT = Mathf.Clamp01((elapsedTime + deltaT) / moveDuration);
        float nextHeightT = _curve.Evaluate(nextLinearT);
        float nextHeight = Mathf.Lerp(0f, _peekHeight, nextHeightT);
        Vector2 nextPos = Vector2.Lerp(startPos, currentTarget.transform.position, nextLinearT) + Vector2.up * nextHeight;
        Vector2 direction = (nextPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    protected override void DealDamage()
    {
        // 직접 데미지
        base.DealDamage();

        // 지속 데미지
        var gridMovement = currentTarget.GetComponent<GridMovement>();
        if (gridMovement != null)
        {
            var originCell = gridMovement.CurrGridCell;

            var directions = new (int x, int y)[]
            {
                (0, 0),    // origin
                (-1, 0),   // left
                (1, 0),    // right
                (0, -1),   // down
                (0, 1)     // up
            };

            foreach (var dir in directions)
            {
                var cell = DefenceContext.Current.GridMap.GetCell(originCell.cellPosition.x + dir.x, originCell.cellPosition.y + dir.y);
                if (cell == null) continue;

                // 1) 지속 데미지 부여
                cell.monstersInCell.ForEach(monster => { damager.DealTickDamage(monster, Status.BURN, 5, 1f, 5); });

                // 2) 셀 시각 효과
                cell.transform?.DOScale(1.5f, 0.25f).SetEase(Ease.OutBack).OnComplete(() => { cell.transform.DOScale(1f, 0.25f).SetEase(Ease.InBack); });
            }
        }
    }
}

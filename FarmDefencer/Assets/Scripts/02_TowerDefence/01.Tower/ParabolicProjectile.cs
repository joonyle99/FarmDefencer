using UnityEngine;
using DG.Tweening;

/// <summary>
/// 포물선 투사체 탄환
/// TODO: 이후에 FireProjectile을 만들어서 해당 로직을 이동시킬 것
/// </summary>
public class ParabolicProjectile : ProjectileBase
{
    [Header("──────── Parabolic Projectile ────────")]
    [Space]

    [SerializeField] private float _peekHeight = 1.5f;
    [SerializeField] private AnimationCurve _curve;

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
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        float rotateSpeed = 20f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }
    protected override void DealDamage()
    {
        // 직접 데미지
        damager.DealDamage(currentTarget, ProjectileType.Fire);

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
    protected override void DealEffect()
    {
        // do nothing
    }
}

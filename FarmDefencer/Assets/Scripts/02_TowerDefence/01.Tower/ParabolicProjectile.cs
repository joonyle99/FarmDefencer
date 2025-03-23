using UnityEngine;

/// <summary>
/// 포물선 투사체 탄환
/// </summary>
public sealed class ParabolicProjectile : ProjectileBase
{
    [Header("──────── Parabolic Projectile ────────")]
    [Space]

    public float moveDuration = 1f;

    private Vector3 startPos;
    private Transform _controlPoint;

    private float _elapsedTime = 0f;

    private void Start()
    {
        startPos = transform.position;
    }
    
    public void SetControlPoint(Transform controlPoint)
    {
        _controlPoint = controlPoint;
    }

    protected override void Move()
    {
        float t = Mathf.Clamp01(JoonyleGameDevKit.Mathematics.EaseOutBack(_elapsedTime / moveDuration));
        transform.position = JoonyleGameDevKit.Mathematics.CalcBezier2Point(startPos, _controlPoint.position, currentTarget.transform.position, t);
        _elapsedTime += Time.deltaTime;
    }
    protected override void DealDamage()
    {
        base.DealDamage();

        // 여기서 불태우는 효과를 추가한다

        // 양쪽 2칸의 몬스터를 불태운다
        // 
        // 현재 타겟이 속해있는 그리드 셀을 찾는다
        // 그 셀을 기준으로 양쪽 2칸의 셀을 찾는다
        //var gridMovement = currentTarget.GetComponent<GridMovement>();
        //var currGridCell = gridMovement.CurrGridCell;
        //var monster = currGridCell.GetMonsterIn();
        //damager.HasDamaged(monster);
    }
}

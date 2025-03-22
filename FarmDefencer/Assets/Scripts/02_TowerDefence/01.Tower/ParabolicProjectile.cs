using UnityEngine;

/// <summary>
/// 포물선 투사체 탄환
/// </summary>
public sealed class ParabolicProjectile : ProjectileBase
{
    [Header("──────── Parabolic Projectile ────────")]
    [Space]

    public float moveDuration = 1f;

    // muzzle(this.transform) -> _controlPoint -> currentTarget 이 순서로 이동하도록 해야함
    private Vector3 startPos;
    private Transform _controlPoint;

    private float _elapsedTime = 0f;

    // TODO: 양쪽 2칸의 몬스터를 불태운다
    // TODO: 불타는 효과를 추가한다

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
        if (_elapsedTime >= moveDuration)
        {
            //damager.HasDamaged(currentTarget);
            //Destroy(gameObject);
            return;
        }

        float t = Mathf.Clamp01(JoonyleGameDevKit.Mathematics.EaseOutBack(_elapsedTime / moveDuration));
        transform.position = JoonyleGameDevKit.Mathematics.CalcBezier2Point(startPos, _controlPoint.position, currentTarget.transform.position, t);
        _elapsedTime += Time.deltaTime;
    }

    //protected override IEnumerator StackCoroutine(Transform _controlPoint, Transform itemOriginPoint, int stackCount)
    //{
    //    // 위치와 회전값은 로컬 좌표계로 동작
    //    Vector3 startLocalPosition = transform.localPosition;
    //    Quaternion startLocalRotation = transform.localRotation;
    //    Vector3 endLocalPosition = itemOriginPoint.localPosition + itemOriginPoint.up * intervalDist * (stackCount - 1);
    //    Quaternion endLocalRotation = Quaternion.Euler(0f, 90f, 0f);

    //    float _elapsedTime = 0f;

    //    while (_elapsedTime < moveDuration)
    //    {
    //        // easing graph
    //        float t = JoonyleGameDevKit.Math.EaseOutBack(_elapsedTime / moveDuration);

    //        // bezier curve
    //        transform.localPosition = JoonyleGameDevKit.Math.CalcBezier2Point(startLocalPosition, _controlPoint.localPosition, endLocalPosition, t);
    //        transform.localRotation = Quaternion.Slerp(startLocalRotation, endLocalRotation, t);

    //        _elapsedTime += Time.deltaTime;

    //        yield return null;
    //    }

    //    transform.localPosition = endLocalPosition;
    //    transform.localRotation = endLocalRotation;
    //}
}

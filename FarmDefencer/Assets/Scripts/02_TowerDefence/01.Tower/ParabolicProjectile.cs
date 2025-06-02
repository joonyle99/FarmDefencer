using UnityEngine;

/// <summary>
/// 포물선 투사체를 구현
/// </summary>
public abstract class ParabolicProjectile : ProjectileBase
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
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        float rotateSpeed = 20f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }
}

using UnityEngine;

/// <summary>
/// 작물 한 종류를 의미하는 추상 클래스입니다.
/// 위치와 같은 물리적 정보만을 담당하며, 심기와 수확하기라는 간단한 액션의 구현만을 필요로 합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Crop : MonoBehaviour
{
    /// <summary>
    /// 이 Crop이 해당 좌표에 해당하는지 확인합니다.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// 작물을 심고 성장 주기에 진입합니다.
    /// 작물 종류에 따라 심을 수 있는 조건이 다를 수 있습니다.
    /// </summary>
    /// <returns>모든 조건을 만족해 작물을 심은 경우 true, 이외의 경우 false</returns>
    public abstract bool TryPlant();

    /// <summary>
    /// 작물을 수확합니다.
    /// 작물 종류에 수확 조건이 다를 수 있습니다.
    /// </summary>
    /// <returns>모든 조건을 만족해 작물을 수확한 경우 true, 이외의 경우 false</returns>
    public abstract bool TryHarvest();

    /// <summary>
    /// 작물을 성장시킵니다.
    /// 작물 종류에 따라 성장 조건이 다를 수 있습니다.
    /// </summary>
    /// <returns>모든 조건을 만족해 작물을 성장시킨 경우 true, 이외의 경우 false</returns>
    public abstract bool TryGrowUp();
}
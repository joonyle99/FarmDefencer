using UnityEngine;

/// <summary>
/// SingleTap, Hold 등을 처리할 수 있는 인터페이스.
/// 각 이벤트에서 액션을 소비한 경우 true를 반환하여 다른 레이어가 더 이상 이벤트를 처리하지 못하도록 할 것.
/// </summary>
public interface IFarmInputLayer
{
    public const int Priority_PestGiver = 1000;
    public const int Priority_CropGuide = 800;
    public const int Priority_WateringCan = 400;
    public const int Priority_HarvestTutorialGiver = 399;
    public const int Priority_Farm = 100;
    
    int InputPriority { get; }

    bool OnSingleTap(Vector2 worldPosition);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialWorldPosition">최초 홀드가 시작된 월드 위치.</param>
    /// <param name="deltaWorldPosition">최초 홀드 위치로부터의 현재 홀드 위치의 상대값.</param>
    /// <param name="isEnd">마지막 홀드, 즉 손가락이나 마우스 클릭을 해제한 순간인지.</param>
    /// <param name="deltaHoldTime"></param>
    bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime);
}
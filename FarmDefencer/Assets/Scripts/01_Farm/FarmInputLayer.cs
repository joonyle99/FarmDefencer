using UnityEngine;

/// <summary>
/// SingleTap, Hold 등을 처리할 수 있는 인터페이스.
/// </summary>
public interface IFarmInputLayer
{
	void OnSingleTap(Vector2 worldPosition);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="initialWorldPosition">최초 홀드가 시작된 월드 위치.</param>
	/// <param name="deltaWorldPosition">최초 홀드 위치로부터의 현재 홀드 위치의 상대값.</param>
	/// <param name="isEnd">마지막 홀드, 즉 손가락이나 마우스 클릭을 해제한 순간인지.</param>
	/// <param name="deltaHoldTime"></param>
	void OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime);
}

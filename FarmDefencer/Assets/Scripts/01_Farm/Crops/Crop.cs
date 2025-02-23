using UnityEngine;
using System;

public abstract class Crop : MonoBehaviour, IFarmUpdatable
{
	public Func<int, int> HarvestHandler;

	public float WaterWaitingDeadSeconds = 300.0f;
	public float WaterWaitingResetSeconds = 300.0f;

	protected float waterWaitingSeconds;
	protected float growthSeconds;
	protected bool watered;

	public virtual void OnSingleTap(Vector2 position) { }

	/// <summary>
	/// </summary>
	/// <param name="deltaPosition">첫 홀드 위치.</param>
	/// <param name="deltaPosition">첫 홀드 위치와 현재 홀드 위치의 차이.</param>
	/// <param name="isEnd">홀드가 종료되어 마지막 액션인 경우 true.</param>
	/// <param name="deltaHoldTime"></param>
	public virtual void OnSingleHolding(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime) { }

	/// <summary>
	/// 이 작물에 물뿌리개의 물 주기 판정이 가해졌을 때의 동작을 정의합니다.
	/// </summary>
	public virtual void OnWatering() { }

	public abstract void OnFarmUpdate(float deltaTime);
}

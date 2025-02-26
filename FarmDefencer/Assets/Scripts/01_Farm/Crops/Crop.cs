using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Crop : MonoBehaviour, IFarmUpdatable
{
	/// <summary>
	/// CropState의 RemainingQuota를 제외한 필드는 반드시 초기값이 초기 상태를 의미하도록 되어야 한다.<br/>
	/// 예: { IsSeed = true }가 초기 상태 - X<br/>
	/// { IsPlanted = false } 가 초기 상태 - O<br/>
	/// 즉 컴파일러가 생성하는 초기값이 초기 상태여야 함.
	/// </summary>
	protected interface ICommonCropState
	{
		bool Planted { get; set; }
		bool Watered { get; set; }
		float WaterWaitingSeconds { get; set; }
		float GrowthSeconds { get; set; }
		int RemainingQuota { get; set; }
		bool Harvested { get; set; }
	}

	protected const float WaterWaitingDeadSeconds = 300.0f;
	protected const float WaterWaitingResetSeconds = 300.0f;
	protected const float MultipleTouchSecondsCriterion = 0.3f; // 연속 탭 동작 판정 시간. 이 시간 이내로 다시 탭 해야 연속 탭으로 간주됨
	protected const float PlantRubbingCriterion = 0.25f; // 밭 문지르기 동작 판정 기준 (가로 방향 위치 델타)

	private Func<int> _getQuota;
	protected Func<int> GetQuota => _getQuota;

	private Action<int> _notifyQuotaFilled; // Farm, Field 등에서는 FillQuota라는 이름이지만, Crop에서는 동작 중에 이미 Quota를 채우는 동작이 있어 명확한 이름 사용.
	/// <summary>
	/// 주문량을 채운 State에 대해 그 주문량만큼 외부 세계로 전달하는 함수.
	/// 기본적으로 FarmManager에 의해 HarvestInventory에 연결됨.
	/// </summary>
	/// <remarks>반드시 GetQuota()로 여유 주문량을 검증한 후 호출할 것.</remarks>
	protected Action<int> NotifyQuotaFilled => _notifyQuotaFilled;

	public void Init(Func<int> getQuotaFunction, Action<int> notifyQuotaFilledFunction)
	{
		_getQuota = getQuotaFunction;
		_notifyQuotaFilled = notifyQuotaFilledFunction;
	}

	/// <summary>
	/// 한 손가락으로 탭할 때의 동작을 정의.
	/// </summary>
	/// <param name="inputWorldPosition"></param>
	public virtual void OnSingleTap(Vector2 inputWorldPosition) { }

	/// <summary>
	/// 한 손가락으로 꾹 누를 때의 동작을 정의.
	/// </summary>
	/// <param name="deltaWorldPosition">첫 홀드 위치.</param>
	/// <param name="deltaWorldPosition">첫 홀드 위치와 현재 홀드 위치의 차이.</param>
	/// <param name="isEnd">홀드가 종료되어 마지막 액션인 경우 true.</param>
	/// <param name="deltaHoldTime"></param>
	public virtual void OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime) { }

	public virtual void OnWatering() { }

	public abstract void OnFarmUpdate(float deltaTime);


	// 이하 함수 빌딩 블록


	protected static Func<Vector2, Vector2, TState> HandleAction_NotifyFilledQuota_PlayEffectAt<TState>(
		List<(Func<TState, TState, bool>, Action<Vector2, Vector2>)> effects,
		Func<int> getQuotaFunction, Action<int> notifyQuotaFilledFunction,
		Func<TState, TState> transitionFunction,
		TState beforeState)
		where TState : struct, ICommonCropState
	{
		beforeState.RemainingQuota = getQuotaFunction();
		var nextState = transitionFunction(beforeState);
		notifyQuotaFilledFunction(GetQuotaFilled(beforeState, nextState));

		return
			(inputWorldPosition, cropPosition) =>
			{
				PlayEffectAt(effects, beforeState, nextState)(inputWorldPosition, cropPosition);
				return nextState;
			};
	}

	protected static void ApplySprite(Sprite sprite, SpriteRenderer spriteRenderer)
	{
		if (spriteRenderer.sprite != sprite)
		{
			spriteRenderer.sprite = sprite;
		}
	}

	/// <summary>
	/// 이전 상태가 Planted && !Watered일 경우 다음 상태에 Watered = true로 설정하여 반환하는 함수.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="beforeState"></param>
	/// <returns></returns>
	protected static TState Water<TState>(TState beforeState) where TState : struct, ICommonCropState
	{
		var nextState = beforeState;
		if (beforeState.Planted && !beforeState.Watered)
		{
			nextState.Watered = true;
		}
		return nextState;
	}
	protected static TState Plant<TState>(TState beforeState) where TState : struct, ICommonCropState{ beforeState.Planted = true; return beforeState; }
	protected static TState WaitWater<TState>(TState beforeState, float deltaTime) where TState : struct, ICommonCropState { beforeState.WaterWaitingSeconds += deltaTime; return beforeState; }
	protected static TState Grow<TState>(TState beforeState, float deltaTime) where TState : struct, ICommonCropState { beforeState.GrowthSeconds += deltaTime; return beforeState; }
	
	/// <summary>
	/// RemainingQuota를 제외한 모든 값을 초기값으로 설정한 상태를 반환,
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="beforeState"></param>
	/// <returns></returns>
	protected static TState Reset<TState>(TState beforeState) where TState : struct, ICommonCropState
	{
		var nextState = new TState() { RemainingQuota = beforeState.RemainingQuota };
		return nextState;
	}
	protected static TState Harvest<TState>(TState beforeState) where TState : struct, ICommonCropState { beforeState.Harvested = true; return beforeState; }
	protected static TState DoNothing<TState>(TState beforeState) where TState : struct, ICommonCropState => beforeState;
	protected static int GetQuotaFilled<TState>(TState beforeState, TState afterState) where TState : struct, ICommonCropState => beforeState.RemainingQuota - afterState.RemainingQuota;
	protected static Action<Vector2, Vector2> PlayEffectAt<TState>(List<(Func<TState, TState, bool>, Action<Vector2, Vector2>)> effects, TState beforeState, TState afterState)
	{
		var (_, effect) = effects.FirstOrDefault(effect => effect.Item1(beforeState, afterState));
		return effect == null ? (_, _) => { } : effect;
	}
	protected static TState DoNothing_OnSingleHolding<TState>(TState beforeState, Vector2 initialWorldPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime) where TState: struct, ICommonCropState => DoNothing(beforeState);
	protected static TState DoNothing_OnFarmUpdate<TState>(TState beforeState, float deltaTime) where TState: struct, ICommonCropState => DoNothing(beforeState);

	// 이펙트 조건 및 실행 함수

	protected static bool WaterEffectCondition<TState>(TState beforeState, TState afterState) where TState : struct, ICommonCropState => !beforeState.Watered && afterState.Watered;
	protected static void WaterEffect(Vector2 inputWorldPosition, Vector2 cropPosition) => SoundManager.PlaySfx("SFX_water_oneshot");

	protected static bool PlantEffectCondition<TState>(TState beforeState, TState afterState) where TState : struct, ICommonCropState => !beforeState.Planted && afterState.Planted;
	protected static void PlantEffect(Vector2 inputWorldPosition, Vector2 cropPosition)
	{
		SoundManager.PlaySfx("SFX_plant_seed");
		EffectPlayer.PlayTabEffect(inputWorldPosition);
	}
	protected static bool HarvestEffectCondition<TState>(TState beforeState, TState afterState) where TState : struct, ICommonCropState => !beforeState.Harvested && afterState.Harvested;
	protected static void HarvestEffect(Vector2 inputWorldPosition, Vector2 cropPosition)
	{
		EffectPlayer.PlayTabEffect(inputWorldPosition);
		SoundManager.PlaySfx("SFX_harvest");
	}

	/// <summary>
	/// 이전 상태의 RemainingQuota와 count 둘 중 작은 값만큼 다음 상태의 RemainingQuota를 감소시키며,
	/// 만약 감소시킨 값이 최초 요청한 count와 같다면 Reset().
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="beforeState"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	protected static TState FillQuotaUptoAndResetIfEqual<TState>(TState beforeState, int count) where TState : struct, ICommonCropState
	{
		var nextState = beforeState;
		var quotaToFill = Math.Min(count, beforeState.RemainingQuota);

		nextState.RemainingQuota -= quotaToFill;
		if (quotaToFill == count)
		{
			nextState = Reset(nextState);
		}

		return nextState; 
	}

	/// <summary>
	/// 이전 상태의 RemainingQuota >= 1이라면 다음 상태에 1 감소시키며 Reset()함.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="beforeState"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	protected static TState FillQuotaOneAndResetIfSucceeded<TState>(TState beforeState) where TState : struct, ICommonCropState => FillQuotaUptoAndResetIfEqual(beforeState, 1);
}

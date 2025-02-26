using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CropEggplant : Crop
{
	private struct EggplantState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public int RemainingQuota { get; set; }
		public int LeavesDropped { get; set; }
		public float LastSingleTapTime { get; set; }
		public bool TrellisPlaced { get; set; }
	}

	private enum EggplantStage
	{
		Seed,
		Mature,
		Harvested,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeTrellis,
		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,

		Stage3_FullLeaves,
		Stage3_HalfLeaves,
	}

	private const float Stage1_GrowthSeconds = 10.0f;
	private const float Stage2_GrowthSeconds = 10.0f;
	private const int InitialLeavesCount = 2; // 마지막 수확 단계에서의 최초 잎 개수

	[SerializeField] private Sprite _seedSprite;
	[Space]
	[SerializeField] private Sprite _stage1_beforeWaterSprite;
	[SerializeField] private Sprite _stage1_deadSprite;
	[SerializeField] private Sprite _stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage2_beforeTrellisSprite;
	[SerializeField] private Sprite _stage2_beforeWaterSprite;
	[SerializeField] private Sprite _stage2_deadSprite;
	[SerializeField] private Sprite _stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage3_fullLeavesSprite;
	[SerializeField] private Sprite _stage3_halfLeavesSprite;
	[Space]
	[SerializeField] private Sprite _matureSprite;
	[SerializeField] private Sprite _harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private EggplantState _currentState;

	public override void OnWatering()
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			Water,
			_currentState)

			(transform.position, transform.position);
	}

	public override void OnSingleTap(Vector2 worldPosition)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			OnSingleTapFunctions[GetCurrentStage(_currentState)], 
			_currentState)

			(transform.position, transform.position);
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		GetSpriteAndApplyTo(currentStage)(_spriteRenderer);
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
			{
				return OnFarmUpdateFunctions[currentStage](_currentState, deltaTime);
			},
			_currentState)

			(transform.position, transform.position);
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// 이하 함수 빌딩 블록

	private static EggplantStage GetCurrentStage(EggplantState state) => state switch
	{
		{ Planted: false } => EggplantStage.Seed,
		{ Harvested: true } => EggplantStage.Harvested,
		{ LeavesDropped: >= 2 } => EggplantStage.Mature,

		{ LeavesDropped: >= 1 } => EggplantStage.Stage3_HalfLeaves,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => EggplantStage.Stage3_FullLeaves,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, TrellisPlaced: false} => EggplantStage.Stage2_BeforeTrellis,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => EggplantStage.Seed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => EggplantStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => EggplantStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => EggplantStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => EggplantStage.Seed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => EggplantStage.Stage1_Dead,
		{ Watered: true } => EggplantStage.Stage1_Growing,
		{ } => EggplantStage.Stage1_BeforeWater,
	};

	private static readonly Dictionary<EggplantStage, Func<EggplantState, float, EggplantState>> OnFarmUpdateFunctions = new Dictionary<EggplantStage, Func<EggplantState, float, EggplantState>>
	{
		{EggplantStage.Seed, (beforeState, deltaTime) => Reset(beforeState) },

		{EggplantStage.Stage1_Dead, WaitWater },
		{EggplantStage.Stage1_BeforeWater, WaitWater },
		{
			EggplantStage.Stage1_Growing,
			(beforeState, deltaTime) =>
			{
				var nextState = Grow(beforeState, deltaTime);
				if (nextState.GrowthSeconds >= Stage1_GrowthSeconds)
				{
					nextState.Watered = false;
				}
				return nextState;
			}
		},

		{EggplantStage.Stage2_BeforeTrellis, DoNothing_OnFarmUpdate },
		{EggplantStage.Stage2_Dead, WaitWater },
		{EggplantStage.Stage2_BeforeWater, WaitWater },
		{EggplantStage.Stage2_Growing, Grow },

		{EggplantStage.Stage3_FullLeaves, DoNothing_OnFarmUpdate },
		{EggplantStage.Stage3_HalfLeaves, DoNothing_OnFarmUpdate },

		{EggplantStage.Mature, DoNothing_OnFarmUpdate },
		{EggplantStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<EggplantStage, Func<EggplantState, EggplantState>> OnSingleTapFunctions = new Dictionary<EggplantStage, Func<EggplantState, EggplantState>>
	{
		{EggplantStage.Seed, Plant },

		{EggplantStage.Stage1_Dead, DoNothing },
		{EggplantStage.Stage1_BeforeWater, DoNothing },
		{EggplantStage.Stage1_Growing, DoNothing },

		{EggplantStage.Stage2_BeforeTrellis, (beforeState) => { beforeState.TrellisPlaced = true; return beforeState; } },
		{EggplantStage.Stage2_BeforeWater, DoNothing },
		{EggplantStage.Stage2_Dead, DoNothing },
		{EggplantStage.Stage2_Growing, DoNothing },

		{EggplantStage.Stage3_FullLeaves, DropLeafIfDoubleTap },
		{EggplantStage.Stage3_HalfLeaves, DropLeafIfDoubleTap },
	
		{EggplantStage.Mature, DoNothing },
		{EggplantStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, 1) },
	};

	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo(EggplantStage stage) => stage switch
	{
		EggplantStage.Seed => (spriteRenderer) => ApplySprite(_seedSprite, spriteRenderer),

		EggplantStage.Stage1_Dead => (spriteRenderer) => ApplySprite(_stage1_deadSprite, spriteRenderer),
		EggplantStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(_stage1_beforeWaterSprite, spriteRenderer),
		EggplantStage.Stage1_Growing => (spriteRenderer) => ApplySprite(_stage1_growingSprite, spriteRenderer),

		EggplantStage.Stage2_BeforeTrellis => (spriteRenderer) => ApplySprite(_stage2_beforeTrellisSprite, spriteRenderer),
		EggplantStage.Stage2_Dead => (spriteRenderer) => ApplySprite(_stage2_deadSprite, spriteRenderer),
		EggplantStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(_stage2_beforeWaterSprite, spriteRenderer),
		EggplantStage.Stage2_Growing => (spriteRenderer) => ApplySprite(_stage2_growingSprite, spriteRenderer),

		EggplantStage.Stage3_FullLeaves => (spriteRenderer) => ApplySprite(_stage3_fullLeavesSprite, spriteRenderer),
		EggplantStage.Stage3_HalfLeaves => (spriteRenderer) => ApplySprite(_stage3_halfLeavesSprite, spriteRenderer),

		EggplantStage.Mature => (spriteRenderer) => ApplySprite(_matureSprite, spriteRenderer),
		EggplantStage.Harvested => (spriteRenderer) => ApplySprite(_harvestedSprite, spriteRenderer),

		_ => (_) => { }
	};

	private static List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect)
	};

	private static EggplantState DropLeafIfDoubleTap(EggplantState beforeState)
	{
		var nextState = beforeState;
		var currentTime = Time.time;
		var lastTapTime = beforeState.LastSingleTapTime;

		nextState.LastSingleTapTime = currentTime;
		if (currentTime < lastTapTime + MultipleTouchSecondsCriterion)
		{
			// 연속 탭 판정 방지
			nextState.LastSingleTapTime -= 2 * MultipleTouchSecondsCriterion;
			nextState.LeavesDropped = beforeState.LeavesDropped + 1;
		}

		return nextState;
	}
}


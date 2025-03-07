using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CropCucumber : Crop
{
	private struct CucumberState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int RemainingQuota { get; set; }
		public bool ShortTrelisPlaced { get; set; }
		public bool LongtrelisPlaced { get; set; }
	}

	private enum CucumberStage
	{
		Seed,
		Mature,
		Harvested,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeShortTrelis,
		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,

		Stage3
	}

	private const float Stage1_GrowthSeconds = 15.0f;
	private const float Stage2_GrowthSeconds = 15.0f;

	[SerializeField] private Sprite _seedSprite;
	[Space]
	[SerializeField] private Sprite _stage1_beforeWaterSprite;
	[SerializeField] private Sprite _stage1_deadSprite;
	[SerializeField] private Sprite _stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage2_beforeShorttrelisSprite;
	[SerializeField] private Sprite _stage2_beforeWaterSprite;
	[SerializeField] private Sprite _stage2_deadSprite;
	[SerializeField] private Sprite _stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage3_sprite;
	[Space]
	[SerializeField] private Sprite _matureSprite;
	[SerializeField] private Sprite _harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private CucumberState _currentState;

	public override void OnWatering()
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
			{
				var nextState = beforeState;
				if (beforeState.Planted
				&& !beforeState.Watered
				&& (beforeState.GrowthSeconds < Stage1_GrowthSeconds || beforeState.ShortTrelisPlaced))
				{
					nextState.Watered = true;
				}
				return nextState;
			},
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

			(worldPosition, transform.position);
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		GetSpriteAndApplyTo(currentStage)(_spriteRenderer);
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

		Effects,
		GetQuota,
		NotifyQuotaFilled,
		(beforeState) =>
		{
			return OnFarmUpdateFunctions[currentStage](beforeState, deltaTime);
		},
		_currentState)

		(transform.position, transform.position);
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}


	// 이하 함수 빌딩 블록

	private static CucumberStage GetCurrentStage(CucumberState state) => state switch
	{
		{ Planted: false } => CucumberStage.Seed,
		{ Harvested: true } => CucumberStage.Harvested,
		{ LongtrelisPlaced: true } => CucumberStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => CucumberStage.Stage3,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, ShortTrelisPlaced: false } => CucumberStage.Stage2_BeforeShortTrelis,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CucumberStage.Seed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CucumberStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => CucumberStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => CucumberStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CucumberStage.Seed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CucumberStage.Stage1_Dead,
		{ Watered: true } => CucumberStage.Stage1_Growing,
		{ } => CucumberStage.Stage1_BeforeWater,
	};

	private static readonly Dictionary<CucumberStage, Func<CucumberState, float, CucumberState>> OnFarmUpdateFunctions = new Dictionary<CucumberStage, Func<CucumberState, float, CucumberState>>
	{
		{CucumberStage.Seed, (beforeState, deltaTime) => Reset(beforeState) },

		{CucumberStage.Stage1_Dead, WaitWater },
		{CucumberStage.Stage1_BeforeWater, WaitWater },
		{
			CucumberStage.Stage1_Growing,
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

		{CucumberStage.Stage2_BeforeShortTrelis, (beforeState, deltaTime) => DoNothing(beforeState) },
		{CucumberStage.Stage2_Dead, WaitWater },
		{CucumberStage.Stage2_BeforeWater, WaitWater },
		{CucumberStage.Stage2_Growing, Grow },

		{CucumberStage.Stage3, (beforeState, deltaTime) => DoNothing(beforeState) },

		{CucumberStage.Mature, (beforeState, deltaTime) => DoNothing(beforeState) },
		{CucumberStage.Harvested, (beforeState, deltaTime) => DoNothing(beforeState) },
	};

private static readonly Dictionary<CucumberStage, Func<CucumberState, CucumberState>> OnSingleTapFunctions = new Dictionary<CucumberStage, Func<CucumberState, CucumberState>>
	{
		{CucumberStage.Seed, Plant },

		{CucumberStage.Stage1_Dead, DoNothing },
		{CucumberStage.Stage1_BeforeWater, DoNothing },
		{CucumberStage.Stage1_Growing, DoNothing },

		{CucumberStage.Stage2_BeforeShortTrelis, (beforeState) => { beforeState.ShortTrelisPlaced = true; return beforeState; } },
		{CucumberStage.Stage2_Dead, DoNothing },
		{CucumberStage.Stage2_BeforeWater, DoNothing },
		{CucumberStage.Stage2_Growing, DoNothing },

		{CucumberStage.Stage3, (beforeState) => { beforeState.LongtrelisPlaced = true; return beforeState; } },

		{CucumberStage.Mature, Harvest },
		{CucumberStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, 1) },
	};

	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo(CucumberStage cucumberStage) => cucumberStage switch
	{
		CucumberStage.Seed => (spriteRenderer) => ApplySprite(_seedSprite, spriteRenderer),

		CucumberStage.Stage1_Dead => (spriteRenderer) => ApplySprite(_stage1_deadSprite, spriteRenderer),
		CucumberStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(_stage1_beforeWaterSprite, spriteRenderer),
		CucumberStage.Stage1_Growing => (spriteRenderer) => ApplySprite(_stage1_growingSprite, spriteRenderer),

		CucumberStage.Stage2_BeforeShortTrelis => (spriteRenderer) => ApplySprite(_stage2_beforeShorttrelisSprite, spriteRenderer),
		CucumberStage.Stage2_Dead => (spriteRenderer) => ApplySprite(_stage2_deadSprite, spriteRenderer),
		CucumberStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(_stage2_beforeWaterSprite, spriteRenderer),
		CucumberStage.Stage2_Growing => (spriteRenderer) => ApplySprite(_stage2_growingSprite, spriteRenderer),

		CucumberStage.Stage3 => (spriteRenderer) => ApplySprite(_stage3_sprite, spriteRenderer),

		CucumberStage.Mature => (spriteRenderer) => ApplySprite(_matureSprite, spriteRenderer),
		CucumberStage.Harvested => (spriteRenderer) => ApplySprite(_harvestedSprite, spriteRenderer),
		_ => (_) => { }
	};

	private static readonly Func<CucumberState, CucumberState, bool> TrelisEffectCondition = (beforeState, afterState) => afterState.LongtrelisPlaced && !beforeState.LongtrelisPlaced || afterState.ShortTrelisPlaced && !beforeState.ShortTrelisPlaced;
	private static readonly Action<Vector2, Vector2> TrelisEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.PlayTabEffect(inputWorldPosition);
		EffectPlayer.PlayVfx("VFX_T_SoilParticleWhite", cropPosition);
	};

	private static List<(Func<CucumberState, CucumberState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<CucumberState, CucumberState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect_SoilDust),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(TrelisEffectCondition, TrelisEffect),
	};
}


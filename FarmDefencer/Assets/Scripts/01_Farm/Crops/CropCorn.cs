using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using UnityEngine;

public class CropCorn : Crop
{
	private struct CornState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int RemainingQuota { get; set; }
	}

	private enum CornStage
	{
		Seed,
		Mature,
		Harvested,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,
	}

	private const float Stage1_GrowthSeconds = 15.0f;
	private const float Stage2_GrowthSeconds = 15.0f;

	[SerializeField] private Sprite _seedSprite;
	[Space]
	[SerializeField] private Sprite _stage1_beforeWaterSprite;
	[SerializeField] private Sprite _stage1_deadSprite;
	[SerializeField] private Sprite _stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage2_beforeWaterSprite;
	[SerializeField] private Sprite _stage2_deadSprite;
	[SerializeField] private Sprite _stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite _matureSprite;
	[SerializeField] private Sprite _harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private CornState _currentState;

	public override void OnSingleTap(Vector2 inputWorldPosition)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(
			Effects, 
			GetQuota,
			NotifyQuotaFilled,
			OnSingleTapFunctions[GetCurrentStage(_currentState)],
			_currentState)

			(inputWorldPosition, transform.position);
	}

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

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		ApplySpriteTo(currentStage)(_spriteRenderer);
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

	private static CornStage GetCurrentStage(CornState state) => state switch
	{
		{ Planted: false } => CornStage.Seed,
		{ Harvested: true } => CornStage.Harvested,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => CornStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds} => CornStage.Seed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CornStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => CornStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => CornStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CornStage.Seed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CornStage.Stage1_Dead,
		{ Watered: true } => CornStage.Stage1_Growing,
		{ } => CornStage.Stage1_BeforeWater,
	};

	private static readonly Dictionary<CornStage, Func<CornState, float, CornState>> OnFarmUpdateFunctions = new Dictionary<CornStage, Func<CornState, float, CornState>>
	{
		{CornStage.Seed, (currentState, deltaTime) => Reset(currentState) },
		{CornStage.Mature, (currentState, deltaTime) => DoNothing(currentState) },
		{CornStage.Harvested, (currentState, deltaTime) => DoNothing(currentState) },

		{CornStage.Stage2_Dead, WaitWater },
		{CornStage.Stage2_BeforeWater, WaitWater },
		{CornStage.Stage2_Growing, Grow },

		{CornStage.Stage1_Dead, WaitWater },
		{CornStage.Stage1_BeforeWater, WaitWater },
		{
			CornStage.Stage1_Growing,
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
	};

	private static readonly Dictionary<CornStage, Func<CornState, CornState>> OnSingleTapFunctions = new Dictionary<CornStage, Func<CornState, CornState>>
	{
		{CornStage.Seed, Plant },
		{CornStage.Mature, Harvest },
		{CornStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, 1) },

		{CornStage.Stage2_Dead, DoNothing },
		{CornStage.Stage2_BeforeWater, DoNothing },
		{CornStage.Stage2_Growing, DoNothing },

		{CornStage.Stage1_Dead, DoNothing },
		{CornStage.Stage1_BeforeWater, DoNothing },
		{CornStage.Stage1_Growing, DoNothing },
	};

	[Pure]
	private Action<SpriteRenderer> ApplySpriteTo(CornStage stage) => stage switch
	{
		CornStage.Seed when _spriteRenderer.sprite != _seedSprite => (spriteRenderer) => spriteRenderer.sprite = _seedSprite,
		CornStage.Mature when _spriteRenderer.sprite != _matureSprite => (spriteRenderer) => spriteRenderer.sprite = _matureSprite,
		CornStage.Harvested when _spriteRenderer.sprite != _harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = _harvestedSprite,

		CornStage.Stage2_Dead when _spriteRenderer.sprite != _stage2_deadSprite => (spriteRenderer) => spriteRenderer.sprite = _stage2_deadSprite,
		CornStage.Stage2_BeforeWater when _spriteRenderer.sprite != _stage2_beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = _stage2_beforeWaterSprite,
		CornStage.Stage2_Growing when _spriteRenderer.sprite != _stage2_growingSprite => (spriteRenderer) => spriteRenderer.sprite = _stage2_growingSprite,

		CornStage.Stage1_Dead when _spriteRenderer.sprite != _stage1_deadSprite => (spriteRenderer) => spriteRenderer.sprite = _stage1_deadSprite,
		CornStage.Stage1_BeforeWater when _spriteRenderer.sprite != _stage1_beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = _stage1_beforeWaterSprite,
		CornStage.Stage1_Growing when _spriteRenderer.sprite != _stage1_growingSprite => (spriteRenderer) => spriteRenderer.sprite = _stage1_growingSprite,

		_ => (_) => { }
	};

	private static List<(Func<CornState, CornState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<CornState, CornState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
	};
}

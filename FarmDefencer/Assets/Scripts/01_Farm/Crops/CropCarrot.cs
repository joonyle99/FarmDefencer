using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CropCarrot : Crop
{
	private struct CarrotState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int RemainingQuota { get; set; }
	}

	private enum CarrotStage
	{
		Seed,
		BeforeWater,
		Dead,
		Growing,
		Mature,
		Harvested
	}

	private const float MatureSeconds = 15.0f;

	[SerializeField] private Sprite seedSprite;
	[SerializeField] private Sprite matureSprite;
	[SerializeField] private Sprite beforeWaterSprite;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private Sprite growingSprite;
	[SerializeField] private Sprite harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private CarrotState _currentState;

	public override void OnSingleTap(Vector2 inputWorldPosition)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			OnSingleTapFunctions[GetCurrentStage(_currentState)], _currentState)

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
	
	public override void ResetToInitialState() => _currentState = Reset(_currentState);

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		ApplySpriteTo(currentStage)(_spriteRenderer);

		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
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

	private static CarrotStage GetCurrentStage(CarrotState state) => state switch
	{
		{ Planted: false } => CarrotStage.Seed,
		{ Harvested: true } => CarrotStage.Harvested,
		{ GrowthSeconds: >= MatureSeconds } => CarrotStage.Mature,
		{ Watered: true } => CarrotStage.Growing,
		{ Watered: false, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CarrotStage.Seed,
		{ Watered: false, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CarrotStage.Dead,
		{ Watered: false } => CarrotStage.BeforeWater,
	};

	private static readonly Dictionary<CarrotStage, Func<CarrotState, float, CarrotState>> OnFarmUpdateFunctions = new()
	{
		{CarrotStage.Seed, (currentState, deltaTime) => Reset(currentState) },
		{CarrotStage.BeforeWater, WaitWater },
		{CarrotStage.Dead, WaitWater },
		{CarrotStage.Growing, Grow },
		{CarrotStage.Mature, DoNothing_OnFarmUpdate },
		{CarrotStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<CarrotStage, Func<CarrotState, CarrotState>> OnSingleTapFunctions = new()
	{
		{CarrotStage.Seed, Plant },
		{CarrotStage.BeforeWater, DoNothing },
		{CarrotStage.Dead, DoNothing },
		{CarrotStage.Growing, DoNothing },
		{CarrotStage.Mature, Harvest },
		{CarrotStage.Harvested, FillQuotaOneAndResetIfSucceeded },
	};

	[Pure]
	private Action<SpriteRenderer> ApplySpriteTo(CarrotStage stage) => stage switch
	{
		CarrotStage.Seed when _spriteRenderer.sprite != seedSprite => (spriteRenderer) => spriteRenderer.sprite = seedSprite,
		CarrotStage.BeforeWater when _spriteRenderer.sprite != beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = beforeWaterSprite,
		CarrotStage.Dead when _spriteRenderer.sprite != deadSprite => (spriteRenderer) => spriteRenderer.sprite = deadSprite,
		CarrotStage.Growing when _spriteRenderer.sprite != growingSprite => (spriteRenderer) => spriteRenderer.sprite = growingSprite,
		CarrotStage.Mature when _spriteRenderer.sprite != matureSprite => (spriteRenderer) => spriteRenderer.sprite = matureSprite,
		CarrotStage.Harvested when _spriteRenderer.sprite != harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = harvestedSprite,
		_ => (_) => { }
	};

	private static List<(Func<CarrotState, CarrotState, bool>, Action<Vector2, Vector2>)> Effects = new()
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect_SoilDust),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
	};
}

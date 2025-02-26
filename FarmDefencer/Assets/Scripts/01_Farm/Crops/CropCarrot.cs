using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CropCarrot : Crop
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

	[SerializeField] private Sprite _seedSprite;
	[SerializeField] private Sprite _matureSprite;
	[SerializeField] private Sprite _beforeWaterSprite;
	[SerializeField] private Sprite _deadSprite;
	[SerializeField] private Sprite _growingSprite;
	[SerializeField] private Sprite _harvestedSprite;

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

	private static readonly Dictionary<CarrotStage, Func<CarrotState, float, CarrotState>> OnFarmUpdateFunctions = new Dictionary<CarrotStage, Func<CarrotState, float, CarrotState>>
	{
		{CarrotStage.Seed, (currentState, deltaTime) => Reset(currentState) },
		{CarrotStage.BeforeWater, WaitWater },
		{CarrotStage.Dead, WaitWater },
		{CarrotStage.Growing, Grow },
		{CarrotStage.Mature, DoNothing_OnFarmUpdate },
		{CarrotStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<CarrotStage, Func<CarrotState, CarrotState>> OnSingleTapFunctions = new Dictionary<CarrotStage, Func<CarrotState, CarrotState>>
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
		CarrotStage.Seed when _spriteRenderer.sprite != _seedSprite => (spriteRenderer) => spriteRenderer.sprite = _seedSprite,
		CarrotStage.BeforeWater when _spriteRenderer.sprite != _beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = _beforeWaterSprite,
		CarrotStage.Dead when _spriteRenderer.sprite != _deadSprite => (spriteRenderer) => spriteRenderer.sprite = _deadSprite,
		CarrotStage.Growing when _spriteRenderer.sprite != _growingSprite => (spriteRenderer) => spriteRenderer.sprite = _growingSprite,
		CarrotStage.Mature when _spriteRenderer.sprite != _matureSprite => (spriteRenderer) => spriteRenderer.sprite = _matureSprite,
		CarrotStage.Harvested when _spriteRenderer.sprite != _harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = _harvestedSprite,
		_ => (_) => { }
	};

	private static List<(Func<CarrotState, CarrotState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<CarrotState, CarrotState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
	};
}

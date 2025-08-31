using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class CropCarrot : Crop
{
	[Serializable]
	private struct CarrotState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float DecayRatio { get; set; }
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
	
	public override RequiredCropAction RequiredCropAction => GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);

	public override float? GaugeRatio =>
		GetCurrentStage(_currentState) is CarrotStage.Mature or CarrotStage.Harvested
			? 1.0f - _currentState.DecayRatio
			: null;
	
	public override void ApplyCommand(CropCommand cropCommand)
	{
		var currentStage = GetCurrentStage(_currentState);
		
		switch (cropCommand)
		{
			case GrowCommand when currentStage == CarrotStage.Growing:
			{
				_currentState.GrowthSeconds = MatureSeconds;
				break;
			}
			case WaterCommand when currentStage == CarrotStage.BeforeWater:
			{
				_currentState.Watered = true;
				break;
			}
		}
	}

	public override JObject Serialize() => JObject.FromObject(_currentState);

	public override void Deserialize(JObject json)
	{
		var state = JsonConvert.DeserializeObject<CarrotState?>(json.ToString());
		if (state != null)
		{
			_currentState = state.Value;
		}
	}

	public override void OnTap(Vector2 inputWorldPosition)
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnPlanted,
			OnSold,
			OnTapFunctions[GetCurrentStage(_currentState)], 
			_currentState,
			inputWorldPosition, 
			transform.position);
	}

	public override bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnPlanted,
			OnSold,
			OnHoldFunctions[GetCurrentStage(_currentState)], 
			_currentState,
			initialWorldPosition + deltaWorldPosition, 
			transform.position);

		return false;
	}

	public override void OnWatering()
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnPlanted,
			OnSold,
			WaterForNeedOnce,
			_currentState,
			transform.position, transform.position);
	}
	
	public override void ResetToInitialState() => _currentState = ResetCropState(_currentState);

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		ApplySpriteTo(currentStage)(_spriteRenderer);

		_currentState = CommonCropBehavior(
			Effects,
			OnPlanted,
			OnSold,
			beforeState => OnFarmUpdateFunctions[currentStage](beforeState, deltaTime),
			_currentState,
			transform.position, 
			transform.position);
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
		{CarrotStage.Seed, (currentState, _) => ResetCropState(currentState) },
		{CarrotStage.BeforeWater, WaitWater },
		{CarrotStage.Dead, WaitWater },
		{CarrotStage.Growing, Grow },
		{CarrotStage.Mature, Decay },
		{CarrotStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<CarrotStage, Func<CarrotState, CarrotState>> OnTapFunctions = new()
	{
		{CarrotStage.Seed, DoNothing },
		{CarrotStage.BeforeWater, DoNothing },
		{CarrotStage.Dead, DoNothing },
		{CarrotStage.Growing, DoNothing },
		{CarrotStage.Mature, DoNothing },
		{CarrotStage.Harvested, ResetCropState },
	};
	
	private static readonly Dictionary<CarrotStage, Func<CarrotState, CarrotState>> OnHoldFunctions = new()
	{
		{CarrotStage.Seed, Plant },
		{CarrotStage.BeforeWater, DoNothing },
		{CarrotStage.Dead, DoNothing },
		{CarrotStage.Growing, DoNothing },
		{CarrotStage.Mature, Harvest },
		{CarrotStage.Harvested, DoNothing },
	};
	
	private static readonly Dictionary<CarrotStage, Func<CarrotState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{CarrotStage.Seed, _ => RequiredCropAction.SingleTap },
		{CarrotStage.BeforeWater, _ => RequiredCropAction.Water },
		{CarrotStage.Dead, _ => RequiredCropAction.Water },
		{CarrotStage.Growing, _ => RequiredCropAction.None },
		{CarrotStage.Mature, _ => RequiredCropAction.SingleTap },
		{CarrotStage.Harvested, _ => RequiredCropAction.SingleTap },
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
		(HarvestEffectCondition, HarvestEffect_SoilDust)
	};
}

using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.PackageManager;

public class CropPotato : Crop
{
	private struct PotatoState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int RemainingQuota { get; set; }
	}

	private enum PotatoStage
	{
		Seed,
		BeforeWater,
		Dead,
		Growing,
		Mature,
		Harvested
	}

	private const float MatureSeconds = 15.0f;
	private const float HarvestHoldTime = 2.0f;

	[SerializeField] private Sprite _seedSprite;
	[SerializeField] private Sprite _matureSprite;
	[SerializeField] private Sprite _beforeWaterSprite;
	[SerializeField] private Sprite _deadSprite;
	[SerializeField] private Sprite _growingSprite;
	[SerializeField] private Sprite _harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private PotatoState _currentState;

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

	public override void OnSingleHolding(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
			{
				return OnSingleHoldingFunctions[GetCurrentStage(_currentState)](beforeState, initialPosition, deltaPosition, isEnd, deltaHoldTime);
			},
			_currentState)

			(initialPosition + deltaPosition, transform.position);
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

	private static PotatoStage GetCurrentStage(PotatoState state) => state switch
	{
		{ Planted: false } => PotatoStage.Seed,
		{ Harvested: true } => PotatoStage.Harvested,
		{ GrowthSeconds: >= MatureSeconds } => PotatoStage.Mature,
		{ Watered: true } => PotatoStage.Growing,
		{ Watered: false, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => PotatoStage.Seed,
		{ Watered: false, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => PotatoStage.Dead,
		{ Watered: false } => PotatoStage.BeforeWater,
	};

	private static readonly Dictionary<PotatoStage, Func<PotatoState, float, PotatoState>> OnFarmUpdateFunctions = new Dictionary<PotatoStage, Func<PotatoState, float, PotatoState>>
	{
		{PotatoStage.Seed, (currentState, deltaTime) => Reset(currentState) },
		{PotatoStage.BeforeWater, WaitWater },
		{PotatoStage.Dead, WaitWater },
		{PotatoStage.Growing, Grow },
		{PotatoStage.Mature, DoNothing_OnFarmUpdate },
		{PotatoStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<PotatoStage, Func<PotatoState, PotatoState>> OnSingleTapFunctions = new Dictionary<PotatoStage, Func<PotatoState, PotatoState>>
	{
		{PotatoStage.Seed, Plant },
		{PotatoStage.BeforeWater, DoNothing },
		{PotatoStage.Dead, DoNothing },
		{PotatoStage.Growing, DoNothing },
		{PotatoStage.Mature, DoNothing },
		{PotatoStage.Harvested, FillQuotaOneAndResetIfSucceeded },
	};

	private static readonly Dictionary<PotatoStage, Func<PotatoState, Vector2, Vector2, bool, float, PotatoState>> OnSingleHoldingFunctions = new Dictionary<PotatoStage, Func<PotatoState, Vector2, Vector2, bool, float, PotatoState>>
	{
		{PotatoStage.Seed, DoNothing_OnSingleHolding },
		{PotatoStage.BeforeWater, DoNothing_OnSingleHolding },
		{PotatoStage.Dead, DoNothing_OnSingleHolding },
		{PotatoStage.Growing, DoNothing_OnSingleHolding },
		{PotatoStage.Harvested, DoNothing_OnSingleHolding },
		{
			PotatoStage.Mature,
			(beforeState, _, _, isEnd, deltaHoldTime) =>
			{
				var afterState = beforeState;
				afterState.HoldingTime += deltaHoldTime;
				if (afterState.HoldingTime > HarvestHoldTime)
				{
					afterState.Harvested = true;
				}
				if (isEnd)
				{
					afterState.HoldingTime = 0.0f;
				}
				return afterState;
			}
		},
	};

	private static readonly Func<PotatoState, PotatoState, bool> HoldEffectCondition = (beforeState, afterState) => afterState.HoldingTime > beforeState.HoldingTime;
	private static readonly Action<Vector2, Vector2> HoldEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.PlayHoldEffect(inputWorldPosition);
		EffectPlayer.PlayVfx("VFX_T_SoilStone", cropPosition, false);
	};

	private static readonly Func<PotatoState, PotatoState, bool> PlayDustSfxEffectCondition = (beforeState, afterState) => afterState.HoldingTime > 0.0f && beforeState.HoldingTime == 0.0f;
	private static readonly Action<Vector2, Vector2> PlayDustSfxEffect = (inputWorldPosition, cropPosition) =>
	{
		SoundManager.PlaySfxStatic("SFX_T_potato_dust");
	};

	private static readonly Func<PotatoState, PotatoState, bool> StopDustSfxEffectCondition = (beforeState, afterState) => afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	private static readonly Action<Vector2, Vector2> StopDustSfxEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.StopVfx();
		SoundManager.StopCurrentSfxStatic();
	};

	[Pure]
	private Action<SpriteRenderer> ApplySpriteTo(PotatoStage stage) => stage switch
	{
		PotatoStage.Seed when _spriteRenderer.sprite != _seedSprite => (spriteRenderer) => spriteRenderer.sprite = _seedSprite,
		PotatoStage.BeforeWater when _spriteRenderer.sprite != _beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = _beforeWaterSprite,
		PotatoStage.Dead when _spriteRenderer.sprite != _deadSprite => (spriteRenderer) => spriteRenderer.sprite = _deadSprite,
		PotatoStage.Growing when _spriteRenderer.sprite != _growingSprite => (spriteRenderer) => spriteRenderer.sprite = _growingSprite,
		PotatoStage.Mature when _spriteRenderer.sprite != _matureSprite => (spriteRenderer) => spriteRenderer.sprite = _matureSprite,
		PotatoStage.Harvested when _spriteRenderer.sprite != _harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = _harvestedSprite,
		_ => (_) => { }
	};

	private static List<(Func<PotatoState, PotatoState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<PotatoState, PotatoState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HoldEffectCondition, HoldEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(StopDustSfxEffectCondition, StopDustSfxEffect),
		(PlayDustSfxEffectCondition, PlayDustSfxEffect)
	};
}

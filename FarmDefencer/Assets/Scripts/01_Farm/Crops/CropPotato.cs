using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class CropPotato : Crop
{
	[Serializable]
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
	private const float HarvestHoldTime = 0.75f;

	[SerializeField] private Sprite seedSprite;
	[SerializeField] private Sprite matureSprite;
	[SerializeField] private Sprite beforeWaterSprite;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private Sprite growingSprite;
	[SerializeField] private Sprite harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private PotatoState _currentState;

	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);

	public override void ApplyCommand(CropCommand cropCommand)
	{
		var currentStage = GetCurrentStage(_currentState);

		switch (cropCommand)
		{
			case GrowCommand when currentStage == PotatoStage.Growing:
			{
				_currentState.GrowthSeconds = MatureSeconds;
				break;
			}
			case WaterCommand when currentStage == PotatoStage.BeforeWater:
			{
				_currentState.Watered = true;
				break;
			}
		}
	}

	public override JObject Serialize() => JObject.FromObject(_currentState);

	public override void Deserialize(JObject json)
	{
		var state = JsonConvert.DeserializeObject<PotatoState?>(json.ToString());
		if (state != null)
		{
			_currentState = state.Value;
		}
	}

	public override void OnTap(Vector2 inputWorldPosition)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			OnTapFunctions[GetCurrentStage(_currentState)],
			_currentState)

			(inputWorldPosition, transform.position);
	}

	public override void OnHold(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
			{
				return OnHoldFunctions[GetCurrentStage(_currentState)](beforeState, initialPosition, deltaPosition, isEnd, deltaHoldTime);
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
			WaterForNeedOnce,
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

	public override void ResetToInitialState() => _currentState = Reset(_currentState);

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

	private static readonly Dictionary<PotatoStage, Func<PotatoState, float, PotatoState>> OnFarmUpdateFunctions = new()
	{
		{PotatoStage.Seed, (currentState, deltaTime) => Reset(currentState) },
		{PotatoStage.BeforeWater, WaitWater },
		{PotatoStage.Dead, WaitWater },
		{PotatoStage.Growing, Grow },
		{PotatoStage.Mature, DoNothing_OnFarmUpdate },
		{PotatoStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<PotatoStage, Func<PotatoState, PotatoState>> OnTapFunctions = new()
	{
		{PotatoStage.Seed, Plant },
		{PotatoStage.BeforeWater, DoNothing },
		{PotatoStage.Dead, DoNothing },
		{PotatoStage.Growing, DoNothing },
		{PotatoStage.Mature, DoNothing },
		{PotatoStage.Harvested, FillQuotaOneAndResetIfSucceeded },
	};

	private static readonly Dictionary<PotatoStage, Func<PotatoState, Vector2, Vector2, bool, float, PotatoState>> OnHoldFunctions = new()
	{
		{PotatoStage.Seed, DoNothing_OnHold },
		{PotatoStage.BeforeWater, DoNothing_OnHold },
		{PotatoStage.Dead, DoNothing_OnHold },
		{PotatoStage.Growing, DoNothing_OnHold },
		{PotatoStage.Harvested, DoNothing_OnHold },
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

	private static readonly Dictionary<PotatoStage, Func<PotatoState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{PotatoStage.Seed, _ => RequiredCropAction.SingleTap },
		{PotatoStage.BeforeWater, _ => RequiredCropAction.Water },
		{PotatoStage.Dead, _ => RequiredCropAction.Water },
		{PotatoStage.Growing, _ => RequiredCropAction.None },
		{PotatoStage.Mature, _ => RequiredCropAction.Hold_0_75 },
		{PotatoStage.Harvested, _ => RequiredCropAction.SingleTap },
	};

	private static readonly Func<PotatoState, PotatoState, bool> HoldEffectCondition = (beforeState, afterState) => afterState.HoldingTime > beforeState.HoldingTime;
	private static readonly Action<Vector2, Vector2> HoldEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.SceneGlobalInstance.PlayHoldEffect(inputWorldPosition);
		EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilStone", cropPosition, false);
	};

	private static readonly Func<PotatoState, PotatoState, bool> PlayDustSfxEffectCondition = (beforeState, afterState) => afterState.HoldingTime > 0.0f && beforeState.HoldingTime == 0.0f;
	private static readonly Action<Vector2, Vector2> PlayDustSfxEffect = (_, _) =>
	{
		SoundManager.Instance.PlaySfx("SFX_T_potato_dust", SoundManager.Instance.potatoDustVolume);
	};

	private static readonly Func<PotatoState, PotatoState, bool> StopDustSfxEffectCondition = (beforeState, afterState) => afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f && !beforeState.Harvested && !afterState.Harvested;
	private static readonly Action<Vector2, Vector2> StopDustSfxEffect = (_, _) =>
	{
		EffectPlayer.SceneGlobalInstance.StopVfx();
		SoundManager.Instance.StopSfx();
	};

	private static readonly Action<Vector2, Vector2> StopDustEffectAndPlayHarvestSfxEffect = (_, _) =>
	{
		EffectPlayer.SceneGlobalInstance.StopVfx();
		SoundManager.Instance.StopSfx();
		SoundManager.Instance.PlaySfx("SFX_T_harvest", SoundManager.Instance.harvestVolume);
	};

	[Pure]
	private Action<SpriteRenderer> ApplySpriteTo(PotatoStage stage) => stage switch
	{
		PotatoStage.Seed when _spriteRenderer.sprite != seedSprite => (spriteRenderer) => spriteRenderer.sprite = seedSprite,
		PotatoStage.BeforeWater when _spriteRenderer.sprite != beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = beforeWaterSprite,
		PotatoStage.Dead when _spriteRenderer.sprite != deadSprite => (spriteRenderer) => spriteRenderer.sprite = deadSprite,
		PotatoStage.Growing when _spriteRenderer.sprite != growingSprite => (spriteRenderer) => spriteRenderer.sprite = growingSprite,
		PotatoStage.Mature when _spriteRenderer.sprite != matureSprite => (spriteRenderer) => spriteRenderer.sprite = matureSprite,
		PotatoStage.Harvested when _spriteRenderer.sprite != harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = harvestedSprite,
		_ => (_) => { }
	};

	private static List<(Func<PotatoState, PotatoState, bool>, Action<Vector2, Vector2>)> Effects = new()
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HoldEffectCondition, HoldEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(StopDustSfxEffectCondition, StopDustSfxEffect),
		(PlayDustSfxEffectCondition, PlayDustSfxEffect),
		(HarvestEffectCondition, StopDustEffectAndPlayHarvestSfxEffect)
	};
}

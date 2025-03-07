using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CropMushroom : Crop
{
	private struct MushroomState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public int RemainingQuota { get; set; }
		public float LastSingleTapTime { get; set; }
		public int TapCount { get; set; }
		public bool Inoculated { get; set; }
		public float HoldingTime { get; set; }
		public bool IsPoisonous { get; set; }
		public float BoomTimeElapsed { get; set; }
	}

	private enum MushroomStage
	{
		Unplowed,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,

		Stage3_BeforeInoculation,
		Stage3_AfterInoculation,

		Mature,

		Harvested,

		Booming,
	}

	private const float Stage1_GrowthSeconds = 15.0f;
	private const float Stage2_GrowthSeconds = 15.0f;
	private const float Stage3_GrowthSeconds = 10.0f;
	private const float PoisonousProbability = 0.65f;
	private const float BoomTime = 5.0f; // 독버섯 수확 후 터져 없어질때까지 걸리는 시간
	private const float InoculationHoldingTime = 2.0f;

	[Space]
	[SerializeField] private Sprite _stage1_beforeWaterSprite;
	[SerializeField] private Sprite _stage1_deadSprite;
	[SerializeField] private Sprite _stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage2_beforeWaterSprite;
	[SerializeField] private Sprite _stage2_deadSprite;
	[SerializeField] private Sprite _stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage3_beforeInoculationSprite;
	[SerializeField] private Sprite _stage3_afterInoculationSprite;
	[Space]
	[SerializeField] private Sprite _mature_normalSprite;
	[SerializeField] private Sprite _mature_poisonousSprite;
	[Space]
	[SerializeField] private Sprite _harvested_normalSprite;
	[SerializeField] private Sprite _harvested_poisonousSprite;

	private SpriteRenderer _spriteRenderer;
	private MushroomState _currentState;

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
		GetSpriteAndApplyTo(currentStage)(_spriteRenderer);
		GetSpriteAndApplyTo_PoisonousVarying(_currentState)(_spriteRenderer);

		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState)
			=>
			{
				return OnFarmUpdateFunctions[GetCurrentStage(_currentState)](beforeState, deltaTime);
			},
			_currentState)

			(transform.position, transform.position);
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}


	// 이하 함수 빌딩 블록

	private static MushroomStage GetCurrentStage(MushroomState state) => state switch
	{
		{ Planted: false } => MushroomStage.Unplowed,
		{ Harvested: true, IsPoisonous: true } => MushroomStage.Booming,
		{ Harvested: true } => MushroomStage.Harvested,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds } => MushroomStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds, Inoculated: true } => MushroomStage.Stage3_AfterInoculation,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => MushroomStage.Stage3_BeforeInoculation,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => MushroomStage.Unplowed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => MushroomStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => MushroomStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => MushroomStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => MushroomStage.Unplowed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => MushroomStage.Stage1_Dead,
		{ Watered: true } => MushroomStage.Stage1_Growing,
		{ } => MushroomStage.Stage1_BeforeWater,
	};

	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo(MushroomStage stage) => stage switch
	{
		MushroomStage.Unplowed => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		MushroomStage.Stage1_Dead => (spriteRenderer) => ApplySprite(_stage1_deadSprite, spriteRenderer),
		MushroomStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(_stage1_beforeWaterSprite, spriteRenderer),
		MushroomStage.Stage1_Growing => (spriteRenderer) => ApplySprite(_stage1_growingSprite, spriteRenderer),

		MushroomStage.Stage2_Dead => (spriteRenderer) => ApplySprite(_stage2_deadSprite, spriteRenderer),
		MushroomStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(_stage2_beforeWaterSprite, spriteRenderer),
		MushroomStage.Stage2_Growing => (spriteRenderer) => ApplySprite(_stage2_growingSprite, spriteRenderer),

		MushroomStage.Stage3_BeforeInoculation => (spriteRenderer) => ApplySprite(_stage3_beforeInoculationSprite, spriteRenderer),
		MushroomStage.Stage3_AfterInoculation => (spriteRenderer) => ApplySprite(_stage3_afterInoculationSprite, spriteRenderer),

		MushroomStage.Mature => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		MushroomStage.Harvested => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		_ => (_) => { }
	};

	private Action<SpriteRenderer> GetSpriteAndApplyTo_PoisonousVarying(MushroomState state) => state switch
	{
		{ GrowthSeconds: < Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds } => (_) => { }
		,

		{ Harvested: false, IsPoisonous: false } => (spriteRenderer) => ApplySprite(_mature_normalSprite, spriteRenderer),
		{ Harvested: false, IsPoisonous: true } => (spriteRenderer) => ApplySprite(_mature_poisonousSprite, spriteRenderer),
		{ IsPoisonous: false } => (spriteRenderer) => ApplySprite(_harvested_normalSprite, spriteRenderer),
		{ IsPoisonous: true } => (spriteRenderer) => ApplySprite(_harvested_poisonousSprite, spriteRenderer),
	};

	private static readonly Func<MushroomState, MushroomState, bool> HoldEffectCondition = (beforeState, afterState) => afterState.HoldingTime > beforeState.HoldingTime;
	private static readonly Action<Vector2, Vector2> HoldEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.PlayHoldEffect(inputWorldPosition);
	};

	private static readonly Func<MushroomState, MushroomState, bool> SoilStoneEffectCondition = (beforeState, afterState) => HoldEffectCondition(beforeState, afterState) && GetCurrentStage(beforeState) != MushroomStage.Stage3_BeforeInoculation;
	private static readonly Action<Vector2, Vector2> SoilStoneEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.PlayVfx("VFX_T_SoilStone", cropPosition, false);
	};

	private static readonly Func<MushroomState, MushroomState, bool> TapEffectCondition = (beforeState, afterState) => afterState.LastSingleTapTime > beforeState.LastSingleTapTime;
	private static readonly Action<Vector2, Vector2> TapEffect = (inputWorldPosition, cropPosition) => EffectPlayer.PlayTabEffect(inputWorldPosition);

	// 용어 참고: SFX에서의 shot == 코드에서의 inoculation.
	private static readonly Func<MushroomState, MushroomState, bool> PlayShotSfxEffectCondition = (beforeState, afterState) => GetCurrentStage(beforeState) == MushroomStage.Stage3_BeforeInoculation && afterState.HoldingTime > 0.0f && beforeState.HoldingTime == 0.0f;
	private static readonly Action<Vector2, Vector2> PlayShotSfxEffect = (inputWorldPosition, cropPosition) => SoundManager.PlaySfxStatic("SFX_T_mushroom_shot");

	private static readonly Func<MushroomState, MushroomState, bool> StopShotSfxEffectCondition = (beforeState, afterState) => afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	private static readonly Action<Vector2, Vector2> StopShotSfxEffect = (inputWorldPosition, cropPosition) => SoundManager.StopCurrentSfxStatic();

	private static readonly Func<MushroomState, MushroomState, bool> MushroomHarvestEffectCondition = (beforeState, afterState) => afterState.TapCount == 2 && beforeState.TapCount != 2;
	private static readonly Action<Vector2, Vector2> MushroomHarvestEffect = (inputWorldPosition, cropPosition) => EffectPlayer.PlayVfx("VFX_T_SoilDust", cropPosition);

	private static readonly Func<MushroomState, MushroomState, bool> HoldStopEffectCondition = (beforeState, afterState) => afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	private static readonly Action<Vector2, Vector2> HoldStopEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.StopVfx();
	};

	private static readonly List<(Func<MushroomState, MushroomState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<MushroomState, MushroomState, bool>, Action<Vector2, Vector2>)>
	{
		(SoilStoneEffectCondition, SoilStoneEffect),
		(HoldStopEffectCondition, HoldStopEffect),
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(HoldEffectCondition, HoldEffect),
		(TapEffectCondition, TapEffect),
		(PlayShotSfxEffectCondition, PlayShotSfxEffect),
		(StopShotSfxEffectCondition, StopShotSfxEffect),
		(MushroomHarvestEffectCondition, MushroomHarvestEffect),
	};

	private static readonly Func<MushroomState, MushroomState> HarvestOnFiveTap =
		(beforeState) =>
		{
			var nextState = beforeState;
			var currentTime = Time.time;
			var lastSingleTapTime = beforeState.LastSingleTapTime;
			if (currentTime < lastSingleTapTime + MultipleTouchSecondsCriterion)
			{
				nextState.TapCount = beforeState.TapCount + 1;
			}
			else
			{
				nextState.TapCount = 1;
			}
			nextState.LastSingleTapTime = currentTime;
			if (nextState.TapCount >= 5)
			{
				nextState = Harvest(nextState);
			}

			return nextState;
		};

	private static readonly Func<MushroomState, float, MushroomState> Boom =
	(beforeState, deltaTime) =>
	{
		var nextState = beforeState;
		nextState.BoomTimeElapsed += deltaTime;

		if (nextState.BoomTimeElapsed >= BoomTime)
		{
			nextState = Reset(beforeState);
		}

		return nextState;
	};

	private static readonly Func<MushroomState, Vector2, Vector2, bool, float, MushroomState> Inoculate =
	(beforeState, initialWorldPosition, deltaPosition, isEnd, deltaHoldTime) =>
	{
		var nextState = beforeState;
		nextState.HoldingTime += deltaHoldTime;


		if (nextState.HoldingTime >= InoculationHoldingTime)
		{
			nextState.Inoculated = true;
		}

		if (isEnd || nextState.Inoculated)
		{
			nextState.HoldingTime = 0.0f;
		}

		return nextState;
	};

	private static readonly Func<MushroomState, Vector2, Vector2, bool, float, MushroomState> Plow =
	(beforeState, initialWorldPosition, deltaPosition, isEnd, deltaHoldTime) =>
	{
		var nextState = beforeState;
		nextState.HoldingTime = beforeState.HoldingTime + deltaHoldTime;
		if (Mathf.Abs(deltaPosition.x) >= PlowDeltaPositionCrierion)
		{
			nextState.Planted = true;
		}

		if (isEnd || nextState.Planted)
		{
			nextState.HoldingTime = 0.0f;
		}

		return nextState;
	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, float, MushroomState>> OnFarmUpdateFunctions = new Dictionary<MushroomStage, Func<MushroomState, float, MushroomState>>
	{
		{
			MushroomStage.Unplowed, 
			(beforeState, deltaTime) =>
			{
				var holdTime = beforeState.HoldingTime;
				var reset = Reset(beforeState);
				reset.HoldingTime = holdTime;
				return reset;
			}
		},

		{MushroomStage.Stage1_Dead, WaitWater },
		{MushroomStage.Stage1_BeforeWater, WaitWater },
		{
			MushroomStage.Stage1_Growing,
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

		{MushroomStage.Stage2_BeforeWater, WaitWater },
		{MushroomStage.Stage2_Dead, WaitWater },
		{MushroomStage.Stage2_Growing, Grow },

		{MushroomStage.Stage3_BeforeInoculation, DoNothing_OnFarmUpdate },
		{
			MushroomStage.Stage3_AfterInoculation,
			(beforeState, deltaTime) =>
			{
				var nextState = Grow(beforeState, deltaTime);

				if (nextState.GrowthSeconds >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds)
				{
					nextState.IsPoisonous = UnityEngine.Random.Range(0.0f, 1.0f) <= PoisonousProbability;
				}

				return nextState;
			}
		},

		{MushroomStage.Mature, DoNothing_OnFarmUpdate },
		{MushroomStage.Booming, Boom },
		{MushroomStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, MushroomState>> OnSingleTapFunctions = new Dictionary<MushroomStage, Func<MushroomState, MushroomState>>
	{
		{MushroomStage.Unplowed, DoNothing },

		{MushroomStage.Stage1_Dead, DoNothing },
		{MushroomStage.Stage1_BeforeWater, DoNothing },
		{MushroomStage.Stage1_Growing, DoNothing },

		{MushroomStage.Stage2_BeforeWater, DoNothing },
		{MushroomStage.Stage2_Dead, DoNothing },
		{MushroomStage.Stage2_Growing, DoNothing },

		{MushroomStage.Stage3_BeforeInoculation, DoNothing },
		{MushroomStage.Stage3_AfterInoculation, DoNothing },

		{MushroomStage.Mature, HarvestOnFiveTap },
		{MushroomStage.Booming, DoNothing },
		{MushroomStage.Harvested, FillQuotaOneAndResetIfSucceeded },
	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, Vector2, Vector2, bool, float, MushroomState>> OnSingleHoldingFunctions = new Dictionary<MushroomStage, Func<MushroomState, Vector2, Vector2, bool, float, MushroomState>>
	{
		{MushroomStage.Unplowed, Plow },

		{MushroomStage.Stage1_Dead, DoNothing_OnSingleHolding },
		{MushroomStage.Stage1_BeforeWater, DoNothing_OnSingleHolding },
		{MushroomStage.Stage1_Growing, DoNothing_OnSingleHolding },

		{MushroomStage.Stage2_BeforeWater, DoNothing_OnSingleHolding },
		{MushroomStage.Stage2_Dead, DoNothing_OnSingleHolding },
		{MushroomStage.Stage2_Growing, DoNothing_OnSingleHolding },

		{MushroomStage.Stage3_BeforeInoculation, Inoculate },
		{MushroomStage.Stage3_AfterInoculation, DoNothing_OnSingleHolding },

		{MushroomStage.Mature, DoNothing_OnSingleHolding },
		{MushroomStage.Booming, DoNothing_OnSingleHolding },
		{MushroomStage.Harvested, DoNothing_OnSingleHolding },
	};
}

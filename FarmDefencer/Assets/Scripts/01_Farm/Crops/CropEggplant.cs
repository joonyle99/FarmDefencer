using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public sealed class CropEggplant : Crop
{
	[Serializable]
	private struct EggplantState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public bool LeavesDropped { get; set; }
		public float LastSingleTapTime { get; set; }
		public bool TrellisPlaced { get; set; }
		public float DecayRatio { get; set; }
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

		Stage3,
		
		Stage4,
	}

	private const float Stage1_GrowthSeconds = 2.0f;
	private const float Stage2_GrowthSeconds = 2.0f;
	private const float Stage4_GrowthSeconds = 2.0f;

	[SerializeField] private Sprite seedSprite;
	[Space]
	[SerializeField] private Sprite stage1_beforeWaterSprite;
	[SerializeField] private Sprite stage1_deadSprite;
	[SerializeField] private Sprite stage1_growingSprite;
	[FormerlySerializedAs("stage2_beforetrelisSprite")]
	[Space]
	[SerializeField] private Sprite stage2_beforeTrellisSprite;
	[SerializeField] private Sprite stage2_beforeWaterSprite;
	[SerializeField] private Sprite stage2_deadSprite;
	[SerializeField] private Sprite stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite stage3_sprite;
	[Space]
	[SerializeField] private Sprite stage4_sprite;
	[Space]
	[SerializeField] private Sprite matureSprite;
	[SerializeField] private Sprite harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private EggplantState _currentState;

	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);
	
	protected override int HarvestableCount => _currentState.Harvested ? 100 : 0;

	public override float? GaugeRatio =>
		GetCurrentStage(_currentState) is EggplantStage.Mature or EggplantStage.Harvested
			? 1.0f - _currentState.DecayRatio
			: null;

	public override void ApplyCommand(CropCommand cropCommand)
	{
		var currentStage = GetCurrentStage(_currentState);

		switch (cropCommand)
		{
			case GrowCommand when currentStage is EggplantStage.Stage1_Growing:
			{
				_currentState.GrowthSeconds = Stage1_GrowthSeconds;
				break;
			}
			case GrowCommand when currentStage is EggplantStage.Stage2_Growing:
			{
				_currentState.GrowthSeconds = Stage2_GrowthSeconds;
				break;
			}
			case WaterCommand when currentStage is EggplantStage.Stage1_BeforeWater or EggplantStage.Stage2_BeforeWater:
			{
				_currentState.Watered = true;
				break;
			}
		}
	}

	public override JObject Serialize() => JObject.FromObject(_currentState);

	public override void Deserialize(JObject json)
	{
		var state = JsonConvert.DeserializeObject<EggplantState?>(json.ToString());
		if (state != null)
		{
			_currentState = state.Value;
		}
	}
	
	public override void OnTap(Vector2 worldPosition)
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnTapFunctions[GetCurrentStage(_currentState)],
			_currentState,
			worldPosition);
	}

	public override bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnHoldFunctions[GetCurrentStage(_currentState)],
			_currentState,
			initialWorldPosition + deltaWorldPosition);

		return false;
	}

	public override void OnWatering()
	{
		_currentState = CommonCropBehavior(
			Effects,
			OnWateringFunctions[GetCurrentStage(_currentState)],
			_currentState,
			transform.position);
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		GetSpriteAndApplyTo(currentStage)(_spriteRenderer);
		_currentState = CommonCropBehavior(

			Effects,
			beforeState => OnFarmUpdateFunctions[currentStage](beforeState, deltaTime),
			_currentState,
			transform.position);
	}

	public override void ResetToInitialState() => _currentState = ResetCropState(_currentState);

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// 이하 함수 빌딩 블록

	private static EggplantStage GetCurrentStage(EggplantState state) => state switch
	{
		{ Planted: false } => EggplantStage.Seed,
		{ Harvested: true } => EggplantStage.Harvested,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage4_GrowthSeconds } => EggplantStage.Mature,

		{ LeavesDropped: true } => EggplantStage.Stage4,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => EggplantStage.Stage3,

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

	private static readonly Dictionary<EggplantStage, Func<EggplantState, float, EggplantState>> OnFarmUpdateFunctions = new()
	{
		{EggplantStage.Seed, (beforeState, deltaTime) => ResetCropState(beforeState) },

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

		{EggplantStage.Stage3, DoNothing_OnFarmUpdate },
		{EggplantStage.Stage4, Grow },

		{EggplantStage.Mature, Decay },
		{EggplantStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<EggplantStage, Func<EggplantState, EggplantState>> OnTapFunctions = new()
	{
		{EggplantStage.Seed, DoNothing },

		{EggplantStage.Stage1_Dead, DoNothing },
		{EggplantStage.Stage1_BeforeWater, DoNothing },
		{EggplantStage.Stage1_Growing, DoNothing },

		{EggplantStage.Stage2_BeforeTrellis, (beforeState) => { beforeState.TrellisPlaced = true; return beforeState; } },
		{EggplantStage.Stage2_BeforeWater, DoNothing },
		{EggplantStage.Stage2_Dead, DoNothing },
		{EggplantStage.Stage2_Growing, DoNothing },

		{EggplantStage.Stage3, DropLeafIfDoubleTap },
		{EggplantStage.Stage4, DoNothing },

		{EggplantStage.Mature, DoNothing },
		{EggplantStage.Harvested, ResetCropState },
	};
	
	private static readonly Dictionary<EggplantStage, Func<EggplantState, EggplantState>> OnHoldFunctions = new()
	{
		{EggplantStage.Seed, Plant },

		{EggplantStage.Stage1_Dead, DoNothing },
		{EggplantStage.Stage1_BeforeWater, DoNothing },
		{EggplantStage.Stage1_Growing, DoNothing },

		{EggplantStage.Stage2_BeforeTrellis, DoNothing },
		{EggplantStage.Stage2_BeforeWater, DoNothing },
		{EggplantStage.Stage2_Dead, DoNothing },
		{EggplantStage.Stage2_Growing, DoNothing },

		{EggplantStage.Stage3, DoNothing },
		{EggplantStage.Stage4, DoNothing },

		{EggplantStage.Mature, Harvest },
		{EggplantStage.Harvested, DoNothing },
	};

	private static readonly Dictionary<EggplantStage, Func<EggplantState, EggplantState>> OnWateringFunctions = new()
	{
		{EggplantStage.Seed, DoNothing },

		{EggplantStage.Stage1_Dead, Water },
		{EggplantStage.Stage1_BeforeWater, Water },
		{EggplantStage.Stage1_Growing, DoNothing },

		{EggplantStage.Stage2_BeforeTrellis, DoNothing },
		{EggplantStage.Stage2_BeforeWater, Water },
		{EggplantStage.Stage2_Dead, Water },
		{EggplantStage.Stage2_Growing, DoNothing },

		{EggplantStage.Stage3, DoNothing },
		{EggplantStage.Stage4, DoNothing },

		{EggplantStage.Mature, DoNothing },
		{EggplantStage.Harvested, DoNothing },
	};
	
	private static readonly Dictionary<EggplantStage, Func<EggplantState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{EggplantStage.Seed, _ => RequiredCropAction.SingleTap },

		{EggplantStage.Stage1_Dead, _ => RequiredCropAction.Water },
		{EggplantStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
		{EggplantStage.Stage1_Growing, _ => RequiredCropAction.None },

		{EggplantStage.Stage2_BeforeTrellis, _ => RequiredCropAction.SingleTap },
		{EggplantStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
		{EggplantStage.Stage2_Dead, _ => RequiredCropAction.Water },
		{EggplantStage.Stage2_Growing, _ => RequiredCropAction.None },

		{EggplantStage.Stage3, _ => RequiredCropAction.DoubleTap },
		{EggplantStage.Stage4, _ => RequiredCropAction.None },

		{EggplantStage.Mature, _ => RequiredCropAction.SingleTap },
		{EggplantStage.Harvested, _ => RequiredCropAction.SingleTap },
	};

	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo(EggplantStage stage) => stage switch
	{
		EggplantStage.Seed => (spriteRenderer) => ApplySprite(seedSprite, spriteRenderer),

		EggplantStage.Stage1_Dead => (spriteRenderer) => ApplySprite(stage1_deadSprite, spriteRenderer),
		EggplantStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(stage1_beforeWaterSprite, spriteRenderer),
		EggplantStage.Stage1_Growing => (spriteRenderer) => ApplySprite(stage1_growingSprite, spriteRenderer),

		EggplantStage.Stage2_BeforeTrellis => (spriteRenderer) => ApplySprite(stage2_beforeTrellisSprite, spriteRenderer),
		EggplantStage.Stage2_Dead => (spriteRenderer) => ApplySprite(stage2_deadSprite, spriteRenderer),
		EggplantStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(stage2_beforeWaterSprite, spriteRenderer),
		EggplantStage.Stage2_Growing => (spriteRenderer) => ApplySprite(stage2_growingSprite, spriteRenderer),

		EggplantStage.Stage3 => (spriteRenderer) => ApplySprite(stage3_sprite, spriteRenderer),
		
		EggplantStage.Stage4 => (spriteRenderer) => ApplySprite(stage4_sprite, spriteRenderer),

		EggplantStage.Mature => (spriteRenderer) => ApplySprite(matureSprite, spriteRenderer),
		EggplantStage.Harvested => (spriteRenderer) => ApplySprite(harvestedSprite, spriteRenderer),

		_ => (_) => { }
	};

	private static readonly Func<EggplantState, EggplantState, bool> TapEffectCondition = (beforeState, afterState) => afterState.LastSingleTapTime > beforeState.LastSingleTapTime;
	private static readonly Action<Vector2, Vector2> TapEffect = (inputWorldPosition, cropPosition) => EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);

	private static readonly Func<EggplantState, EggplantState, bool> TrellisEffectCondition = (beforeState, afterState) => afterState.TrellisPlaced && !beforeState.TrellisPlaced;
	private static readonly Action<Vector2, Vector2> TrellisEffect = (inputWorldPosition, cropPosition) => EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilParticleWhite", cropPosition);

	private static readonly Func<int, Func<EggplantState, EggplantState, bool>> LeafDropSfxEffectConditionFor = (leavesDropped) => (beforeState, afterState) => afterState.LeavesDropped && !beforeState.LeavesDropped;
	private static readonly Func<int, Action<Vector2, Vector2>> LeafDropSfxEffectFor =
		(leavesDropped) =>
		(inputWorldPosition, cropPosition) =>
		{
			SoundManager.Instance.PlaySfx($"SFX_T_eggplant_leaf_{leavesDropped}", SoundManager.Instance.eggPlantLeafDropVolume);
			if (leavesDropped == 1)
			{
				EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDustL", cropPosition);
			}
			else
			{
				EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDustR", cropPosition);
			}
		};


	private static List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect_SoilParticle),
		(TapEffectCondition, TapEffect),
		(TrellisEffectCondition, TrellisEffect),
		(LeafDropSfxEffectConditionFor(1), LeafDropSfxEffectFor(1)),
		(LeafDropSfxEffectConditionFor(2), LeafDropSfxEffectFor(2)),
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
			nextState.LeavesDropped = true;
		}

		return nextState;
	}
}


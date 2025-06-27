using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		public int RemainingQuota { get; set; }
		public int LeavesDropped { get; set; }
		public float LastSingleTapTime { get; set; }
		public bool TrelisPlaced { get; set; }
	}

	private enum EggplantStage
	{
		Seed,
		Mature,
		Harvested,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeTrelis,
		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,

		Stage3_FullLeaves,
		Stage3_HalfLeaves,
	}

	private const float Stage1_GrowthSeconds = 10.0f;
	private const float Stage2_GrowthSeconds = 10.0f;

	[SerializeField] private Sprite seedSprite;
	[Space]
	[SerializeField] private Sprite stage1_beforeWaterSprite;
	[SerializeField] private Sprite stage1_deadSprite;
	[SerializeField] private Sprite stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite stage2_beforetrelisSprite;
	[SerializeField] private Sprite stage2_beforeWaterSprite;
	[SerializeField] private Sprite stage2_deadSprite;
	[SerializeField] private Sprite stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite stage3_fullLeavesSprite;
	[SerializeField] private Sprite stage3_halfLeavesSprite;
	[Space]
	[SerializeField] private Sprite matureSprite;
	[SerializeField] private Sprite harvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private EggplantState _currentState;
	
	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);
	
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
				&& (beforeState.GrowthSeconds < Stage1_GrowthSeconds || beforeState.TrelisPlaced))
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

	private static EggplantStage GetCurrentStage(EggplantState state) => state switch
	{
		{ Planted: false } => EggplantStage.Seed,
		{ Harvested: true } => EggplantStage.Harvested,
		{ LeavesDropped: >= 2 } => EggplantStage.Mature,

		{ LeavesDropped: >= 1 } => EggplantStage.Stage3_HalfLeaves,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => EggplantStage.Stage3_FullLeaves,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, TrelisPlaced: false} => EggplantStage.Stage2_BeforeTrelis,
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

		{EggplantStage.Stage2_BeforeTrelis, DoNothing_OnFarmUpdate },
		{EggplantStage.Stage2_Dead, WaitWater },
		{EggplantStage.Stage2_BeforeWater, WaitWater },
		{EggplantStage.Stage2_Growing, Grow },

		{EggplantStage.Stage3_FullLeaves, DoNothing_OnFarmUpdate },
		{EggplantStage.Stage3_HalfLeaves, DoNothing_OnFarmUpdate },

		{EggplantStage.Mature, DoNothing_OnFarmUpdate },
		{EggplantStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<EggplantStage, Func<EggplantState, EggplantState>> OnSingleTapFunctions = new()
	{
		{EggplantStage.Seed, Plant },

		{EggplantStage.Stage1_Dead, DoNothing },
		{EggplantStage.Stage1_BeforeWater, DoNothing },
		{EggplantStage.Stage1_Growing, DoNothing },

		{EggplantStage.Stage2_BeforeTrelis, (beforeState) => { beforeState.TrelisPlaced = true; return beforeState; } },
		{EggplantStage.Stage2_BeforeWater, DoNothing },
		{EggplantStage.Stage2_Dead, DoNothing },
		{EggplantStage.Stage2_Growing, DoNothing },

		{EggplantStage.Stage3_FullLeaves, DropLeafIfDoubleTap },
		{EggplantStage.Stage3_HalfLeaves, DropLeafIfDoubleTap },
	
		{EggplantStage.Mature, Harvest },
		{EggplantStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, 1) },
	};	
	
	private static readonly Dictionary<EggplantStage, Func<EggplantState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{EggplantStage.Seed, _ => RequiredCropAction.SingleTap },

		{EggplantStage.Stage1_Dead, _ => RequiredCropAction.Water },
		{EggplantStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
		{EggplantStage.Stage1_Growing, _ => RequiredCropAction.None },

		{EggplantStage.Stage2_BeforeTrelis, _ => RequiredCropAction.SingleTap },
		{EggplantStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
		{EggplantStage.Stage2_Dead, _ => RequiredCropAction.Water },
		{EggplantStage.Stage2_Growing, _ => RequiredCropAction.None },

		{EggplantStage.Stage3_FullLeaves, _ => RequiredCropAction.DoubleTap },
		{EggplantStage.Stage3_HalfLeaves, _ => RequiredCropAction.DoubleTap },
	
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

		EggplantStage.Stage2_BeforeTrelis => (spriteRenderer) => ApplySprite(stage2_beforetrelisSprite, spriteRenderer),
		EggplantStage.Stage2_Dead => (spriteRenderer) => ApplySprite(stage2_deadSprite, spriteRenderer),
		EggplantStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(stage2_beforeWaterSprite, spriteRenderer),
		EggplantStage.Stage2_Growing => (spriteRenderer) => ApplySprite(stage2_growingSprite, spriteRenderer),

		EggplantStage.Stage3_FullLeaves => (spriteRenderer) => ApplySprite(stage3_fullLeavesSprite, spriteRenderer),
		EggplantStage.Stage3_HalfLeaves => (spriteRenderer) => ApplySprite(stage3_halfLeavesSprite, spriteRenderer),

		EggplantStage.Mature => (spriteRenderer) => ApplySprite(matureSprite, spriteRenderer),
		EggplantStage.Harvested => (spriteRenderer) => ApplySprite(harvestedSprite, spriteRenderer),

		_ => (_) => { }
	};

	private static readonly Func<EggplantState, EggplantState, bool> TapEffectCondition = (beforeState, afterState) => afterState.LastSingleTapTime > beforeState.LastSingleTapTime;
	private static readonly Action<Vector2, Vector2> TapEffect = (inputWorldPosition, cropPosition) => EffectPlayer.PlayTabEffect(inputWorldPosition);

	private static readonly Func<EggplantState, EggplantState, bool> TrelisEffectCondition = (beforeState, afterState) => afterState.TrelisPlaced && !beforeState.TrelisPlaced;
	private static readonly Action<Vector2, Vector2> TrelisEffect = (inputWorldPosition, cropPosition) => EffectPlayer.PlayVfx("VFX_T_SoilParticleWhite", cropPosition);

	private static readonly Func<int, Func<EggplantState, EggplantState, bool>> LeafDropSfxEffectConditionFor = (leavesDropped) => (beforeState, afterState) => afterState.LeavesDropped == leavesDropped && beforeState.LeavesDropped < leavesDropped;
	private static readonly Func<int, Action<Vector2, Vector2>> LeafDropSfxEffectFor =
		(leavesDropped) =>
		(inputWorldPosition, cropPosition) =>
		{
			SoundManager.Instance.PlaySfx($"SFX_T_eggplant_leaf_{leavesDropped}");
			if (leavesDropped == 1)
			{
				EffectPlayer.PlayVfx("VFX_T_SoilDustL", cropPosition);
			}
			else
			{
				EffectPlayer.PlayVfx("VFX_T_SoilDustR", cropPosition);
			}
		};


	private static List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<EggplantState, EggplantState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect_SoilParticle),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(TapEffectCondition, TapEffect),
		(TrelisEffectCondition, TrelisEffect),
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
			nextState.LeavesDropped = beforeState.LeavesDropped + 1;
		}

		return nextState;
	}
}


using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Spine.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 0번 자식에는 SkeletonGraphic 컴포넌트를 가진 자식을 배치할 것.
/// </summary>
public sealed class CropMushroom : Crop
{
	private enum InoculationState
	{
		ShouldPlaceInjector,
		BeforeInoculation,
		AfterInoculation
	}
	[Serializable]
	private struct MushroomState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int TapCount { get; set; }
		public float LastSingleTapTime { get; set; }
		public InoculationState InoculationState { get; set; }
		public bool IsPoisonous { get; set; }
		public float DecayRatio { get; set; }
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

		Stage3,
		Stage3_BeforeInoculation,
		Stage3_AfterInoculation,

		Mature,

		Harvested,
	}

	private const float Stage1_GrowthSeconds = 2.0f;
	private const float Stage2_GrowthSeconds = 2.0f;
	private const float Stage3_GrowthSeconds = 3.0f;
	private const float PoisonousProbability = 0.65f;
	private const float InoculationHoldingTime = 2.0f;

	[Space]
	[SerializeField] private Sprite stage1_beforeWaterSprite;
	[SerializeField] private Sprite stage1_deadSprite;
	[SerializeField] private Sprite stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite stage2_beforeWaterSprite;
	[SerializeField] private Sprite stage2_deadSprite;
	[SerializeField] private Sprite stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite stage3_sprite;
	[Space]
	[SerializeField] private Sprite mature_normalSprite;
	[SerializeField] private Sprite mature_poisonousSprite;
	[Space]
	[SerializeField] private Sprite harvested_normalSprite;
	[SerializeField] private Sprite harvested_poisonousSprite;

	[Space]

	[SerializeField]
	[SpineAnimation]
	private string idleAnimationName;
	[SerializeField]
	[SpineAnimation]
	private string injectAnimationName;
	[SerializeField]
	[SpineAnimation]
	private string injectFinishedAnimationName;

	private GameObject _inoculationAnimationObject; // 0번 자식
	private SkeletonAnimation _skeletonAnimation; // 0번 자식이 소유
	private Spine.AnimationState _spineAnimationState; // 0번 자식이 소유
	private Spine.Skeleton _skeleton; // 0번 자식이 소유

	private SpriteRenderer _spriteRenderer;
	private MushroomState _currentState;

	public bool ForceHarvestOne { get; set; }

	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);
	
	protected override int HarvestableCount => _currentState is { Harvested: true, IsPoisonous: false } ? 100 : 0;
	
	public override float? GaugeRatio =>
		GetCurrentStage(_currentState) is MushroomStage.Mature or MushroomStage.Harvested && !_currentState.IsPoisonous
			? 1.0f - _currentState.DecayRatio
			: null;

	public override void ApplyCommand(CropCommand cropCommand)
	{
		var currentStage = GetCurrentStage(_currentState);

		switch (cropCommand)
		{
			case GrowCommand when currentStage is MushroomStage.Stage1_Growing:
			{
				_currentState.GrowthSeconds = Stage1_GrowthSeconds;
				break;
			}
			case GrowCommand when currentStage is MushroomStage.Stage2_Growing:
			{
				_currentState.GrowthSeconds = Stage2_GrowthSeconds;
				break;
			}
			case WaterCommand when currentStage is MushroomStage.Stage1_BeforeWater or MushroomStage.Stage2_BeforeWater:
			{
				_currentState.Watered = true;
				break;
			}
		}
	}

	public override JObject Serialize() => JObject.FromObject(_currentState);

	public override void Deserialize(JObject json)
	{
		var state = JsonConvert.DeserializeObject<MushroomState?>(json.ToString());
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

	public override bool OnHold(Vector2 initialPosition, Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		_currentState = CommonCropBehavior(
			Effects,
			beforeState => OnHoldFunctions[GetCurrentStage(_currentState)](beforeState, initialPosition, deltaPosition, isEnd, deltaHoldTime),
			_currentState,
			initialPosition + deltaPosition);

		return true;
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
		GetSpriteAndApplyTo_PoisonousVarying(_currentState)(_spriteRenderer);

		_currentState = CommonCropBehavior(
			Effects,
			beforeState => OnFarmUpdateFunctions[GetCurrentStage(_currentState)](beforeState, deltaTime),
			_currentState,
			transform.position);

		if (ForceHarvestOne && _currentState.IsPoisonous)
		{
			_currentState.IsPoisonous = false;
		}

		RenderInoculationAnimation();
	}

	public override void ResetToInitialState() => _currentState = ResetCropState(_currentState);

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_inoculationAnimationObject = transform.GetChild(0).gameObject;
		_skeletonAnimation = _inoculationAnimationObject.GetComponent<SkeletonAnimation>();
		_spineAnimationState = _skeletonAnimation.AnimationState;
		_skeleton = _skeletonAnimation.Skeleton;
	}

	// 이하 함수 빌딩 블록

	private void RenderInoculationAnimation()
	{
		var currentStage = GetCurrentStage(_currentState);
		var currentAnimationName = _spineAnimationState.GetCurrent(0).Animation.Name;

		if (currentStage == MushroomStage.Stage3_BeforeInoculation)
		{
			_inoculationAnimationObject.SetActive(true);
			if (_currentState.HoldingTime > 0.0f)
			{
				if (currentAnimationName != injectAnimationName)
				{
					_spineAnimationState.SetAnimation(0, injectAnimationName, false);
				}
			}
			else
			{
				if (currentAnimationName != idleAnimationName)
				{
					_spineAnimationState.SetAnimation(0, idleAnimationName, false);
				}
			}
		}
		else if (currentStage == MushroomStage.Stage3_AfterInoculation)
		{
			_inoculationAnimationObject.SetActive(true);
			if (currentAnimationName != injectFinishedAnimationName)
			{
				_spineAnimationState.SetAnimation(0, injectFinishedAnimationName, false);
			}
		}
		else
		{
			_inoculationAnimationObject.SetActive(false);
		}
	}

	private static MushroomStage GetCurrentStage(MushroomState state) => state switch
	{
		{ Planted: false } => MushroomStage.Unplowed,
		{ Harvested: true, IsPoisonous: true } => MushroomStage.Harvested,
		{ Harvested: true } => MushroomStage.Harvested,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds } => MushroomStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds, InoculationState: InoculationState.ShouldPlaceInjector } => MushroomStage.Stage3,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds, InoculationState: InoculationState.BeforeInoculation } => MushroomStage.Stage3_BeforeInoculation,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => MushroomStage.Stage3_AfterInoculation,

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

		MushroomStage.Stage1_Dead => (spriteRenderer) => ApplySprite(stage1_deadSprite, spriteRenderer),
		MushroomStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(stage1_beforeWaterSprite, spriteRenderer),
		MushroomStage.Stage1_Growing => (spriteRenderer) => ApplySprite(stage1_growingSprite, spriteRenderer),

		MushroomStage.Stage2_Dead => (spriteRenderer) => ApplySprite(stage2_deadSprite, spriteRenderer),
		MushroomStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(stage2_beforeWaterSprite, spriteRenderer),
		MushroomStage.Stage2_Growing => (spriteRenderer) => ApplySprite(stage2_growingSprite, spriteRenderer),

		MushroomStage.Stage3 or MushroomStage.Stage3_AfterInoculation or MushroomStage.Stage3_BeforeInoculation => (spriteRenderer) => ApplySprite(stage3_sprite, spriteRenderer),

		MushroomStage.Mature => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		MushroomStage.Harvested => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		_ => (_) => { }
	};

	private Action<SpriteRenderer> GetSpriteAndApplyTo_PoisonousVarying(MushroomState state) => state switch
	{
		{ GrowthSeconds: < Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds } => (_) => { }
		,

		{ Harvested: false, IsPoisonous: false } => (spriteRenderer) => ApplySprite(mature_normalSprite, spriteRenderer),
		{ Harvested: false, IsPoisonous: true } => (spriteRenderer) => ApplySprite(mature_poisonousSprite, spriteRenderer),
		{ IsPoisonous: false } => (spriteRenderer) => ApplySprite(harvested_normalSprite, spriteRenderer),
		{ IsPoisonous: true } => (spriteRenderer) => ApplySprite(harvested_poisonousSprite, spriteRenderer),
	};

	private static readonly Func<MushroomState, MushroomState, bool> HoldEffectCondition = (beforeState, afterState) => afterState.HoldingTime > beforeState.HoldingTime;
	private static readonly Action<Vector2, Vector2> HoldEffect = (inputWorldPosition, _) =>
	{
		EffectPlayer.SceneGlobalInstance.PlayHoldEffect(inputWorldPosition);
	};

	private static readonly Func<MushroomState, MushroomState, bool> SoilStoneEffectCondition = (beforeState, afterState) => HoldEffectCondition(beforeState, afterState) && GetCurrentStage(beforeState) != MushroomStage.Stage3_BeforeInoculation;
	private static readonly Action<Vector2, Vector2> SoilStoneEffect = (_, cropPosition) =>
	{
		EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilStone", cropPosition, false);
	};

	private static readonly Func<MushroomState, MushroomState, bool> TapEffectCondition = (beforeState, afterState) => afterState.LastSingleTapTime > beforeState.LastSingleTapTime;
	private static readonly Action<Vector2, Vector2> TapEffect = (inputWorldPosition, _) => EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);

	// 용어 참고: SFX에서의 shot == 코드에서의 inoculation.
	private static readonly Func<MushroomState, MushroomState, bool> PlayShotSfxEffectCondition = (beforeState, afterState) => GetCurrentStage(beforeState) == MushroomStage.Stage3_BeforeInoculation && afterState.HoldingTime > 0.0f && beforeState.HoldingTime == 0.0f;
	private static readonly Action<Vector2, Vector2> PlayShotSfxEffect = (_, _) => SoundManager.Instance.PlaySfx("SFX_T_mushroom_shot_1", SoundManager.Instance.mushroomShotVolume);

	private static readonly Func<MushroomState, MushroomState, bool> StopShotSfxEffectCondition = (beforeState, afterState) => GetCurrentStage(beforeState) == MushroomStage.Stage3_BeforeInoculation && GetCurrentStage(afterState) == MushroomStage.Stage3_BeforeInoculation && afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	private static readonly Action<Vector2, Vector2> StopShotSfxEffect = (_, _) => SoundManager.Instance.StopSfx();

	private static readonly Func<MushroomState, MushroomState, bool> ShotDoneEffectCondition = (beforeState, afterState) => GetCurrentStage(beforeState) == MushroomStage.Stage3_BeforeInoculation && GetCurrentStage(afterState) == MushroomStage.Stage3_AfterInoculation;
	private static readonly Action<Vector2, Vector2> ShotDoneEffect = (_, _) => SoundManager.Instance.PlaySfx("SFX_T_mushroom_shot_2", SoundManager.Instance.mushroomShotVolume);
	
	private static readonly Func<MushroomState, MushroomState, bool> MushroomHarvestEffectCondition = (beforeState, afterState) => afterState.TapCount == 2 && beforeState.TapCount != 2;
	private static readonly Action<Vector2, Vector2> MushroomHarvestEffect = (_, cropPosition) => EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDust", cropPosition);

	private static readonly Func<MushroomState, MushroomState, bool> HoldStopEffectCondition = (beforeState, afterState) => afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	private static readonly Action<Vector2, Vector2> HoldStopEffect = (_, _) =>
	{
		EffectPlayer.SceneGlobalInstance.StopVfx();
	};
	
	private static readonly Func<MushroomState, MushroomState, bool> Growth1_Mature_PlaceInjector_EffectCondition = (beforeState, afterState) => afterState.GrowthSeconds >= Stage1_GrowthSeconds && beforeState.GrowthSeconds < Stage1_GrowthSeconds || afterState.GrowthSeconds >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds && beforeState.GrowthSeconds < Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds || afterState.InoculationState == InoculationState.BeforeInoculation && beforeState.InoculationState == InoculationState.ShouldPlaceInjector;
	private static readonly Action<Vector2, Vector2> Growth1_Mature_PlaceInjector_Effect = (_, cropWorldPosition) => EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilParticle", cropWorldPosition);
	
	private static readonly Func<MushroomState, MushroomState, bool> Growth2_Inoculation_EffectCondition = (beforeState, afterState) => afterState.GrowthSeconds >= Stage1_GrowthSeconds + Stage2_GrowthSeconds && beforeState.GrowthSeconds < Stage1_GrowthSeconds + Stage2_GrowthSeconds || afterState.InoculationState == InoculationState.AfterInoculation && beforeState.InoculationState == InoculationState.BeforeInoculation;
	private static readonly Action<Vector2, Vector2> Growth2_Inoculation_Effect = (_, cropWorldPosition) => EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDust", cropWorldPosition);

	private static readonly List<(Func<MushroomState, MushroomState, bool>, Action<Vector2, Vector2>)> Effects = new()
	{
		(SoilStoneEffectCondition, SoilStoneEffect),
		(HoldStopEffectCondition, HoldStopEffect),
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HoldEffectCondition, HoldEffect),
		(TapEffectCondition, TapEffect),
		(PlayShotSfxEffectCondition, PlayShotSfxEffect),
		(StopShotSfxEffectCondition, StopShotSfxEffect),
		(ShotDoneEffectCondition, ShotDoneEffect),
		(MushroomHarvestEffectCondition, MushroomHarvestEffect),
		(Growth1_Mature_PlaceInjector_EffectCondition, Growth1_Mature_PlaceInjector_Effect),
		(Growth2_Inoculation_EffectCondition, Growth2_Inoculation_Effect),
	};

	private static readonly Func<MushroomState, MushroomState> HarvestOnTripleTap =
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
			if (nextState.TapCount >= 3)
			{
				nextState = Harvest(nextState);
				if (beforeState.IsPoisonous)
				{
					nextState = ResetCropState(beforeState);
				}
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
			nextState.InoculationState = InoculationState.AfterInoculation;
		}

		if (isEnd || nextState.InoculationState == InoculationState.AfterInoculation)
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
		if (Mathf.Abs(deltaPosition.x) >= PlowDeltaPositionCriterion)
		{
			nextState.Planted = true;
		}

		if (isEnd || nextState.Planted)
		{
			nextState.HoldingTime = 0.0f;
		}

		return nextState;
	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, float, MushroomState>> OnFarmUpdateFunctions = new()
	{
		{
			MushroomStage.Unplowed,
			(beforeState, deltaTime) =>
			{
				var holdTime = beforeState.HoldingTime;
				var reset = ResetCropState(beforeState);
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

		{MushroomStage.Stage3, DoNothing_OnFarmUpdate },
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

		{MushroomStage.Mature, Decay },
		{MushroomStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, MushroomState>> OnTapFunctions = new()
	{
		{MushroomStage.Unplowed, DoNothing },

		{MushroomStage.Stage1_Dead, DoNothing },
		{MushroomStage.Stage1_BeforeWater, DoNothing },
		{MushroomStage.Stage1_Growing, DoNothing },

		{MushroomStage.Stage2_BeforeWater, DoNothing },
		{MushroomStage.Stage2_Dead, DoNothing },
		{MushroomStage.Stage2_Growing, DoNothing },

		{
			MushroomStage.Stage3,
			beforeState =>
			{
				var nextState = beforeState;
				nextState.InoculationState = InoculationState.BeforeInoculation;
				return nextState;
			}
		},
		{MushroomStage.Stage3_BeforeInoculation, DoNothing },
		{MushroomStage.Stage3_AfterInoculation, DoNothing },

		{MushroomStage.Mature, HarvestOnTripleTap },
		{MushroomStage.Harvested, ResetCropState },
	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, MushroomState>> OnWateringFunctions = new()
	{
		{MushroomStage.Unplowed, DoNothing },

		{MushroomStage.Stage1_Dead, Water },
		{MushroomStage.Stage1_BeforeWater, Water },
		{MushroomStage.Stage1_Growing, DoNothing },

		{MushroomStage.Stage2_BeforeWater, Water },
		{MushroomStage.Stage2_Dead, Water },
		{MushroomStage.Stage2_Growing, DoNothing },

		{MushroomStage.Stage3, DoNothing },
		{MushroomStage.Stage3_BeforeInoculation, DoNothing },
		{MushroomStage.Stage3_AfterInoculation, DoNothing },

		{MushroomStage.Mature, DoNothing },
		{MushroomStage.Harvested, DoNothing },
	};
	
	private static readonly Dictionary<MushroomStage, Func<MushroomState, Vector2, Vector2, bool, float, MushroomState>> OnHoldFunctions = new()
	{
		{MushroomStage.Unplowed, Plow },

		{MushroomStage.Stage1_Dead, DoNothing_OnHold },
		{MushroomStage.Stage1_BeforeWater, DoNothing_OnHold },
		{MushroomStage.Stage1_Growing, DoNothing_OnHold },

		{MushroomStage.Stage2_BeforeWater, DoNothing_OnHold },
		{MushroomStage.Stage2_Dead, DoNothing_OnHold },
		{MushroomStage.Stage2_Growing, DoNothing_OnHold },

		{MushroomStage.Stage3, DoNothing_OnHold },
		{MushroomStage.Stage3_BeforeInoculation, Inoculate },
		{MushroomStage.Stage3_AfterInoculation, DoNothing_OnHold },

		{MushroomStage.Mature, DoNothing_OnHold },
		{MushroomStage.Harvested, DoNothing_OnHold },
	};

	private static readonly Dictionary<MushroomStage, Func<MushroomState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{MushroomStage.Unplowed, _ => RequiredCropAction.Drag },

		{MushroomStage.Stage1_Dead, _ => RequiredCropAction.Water },
		{MushroomStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
		{MushroomStage.Stage1_Growing, _ => RequiredCropAction.None },

		{MushroomStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
		{MushroomStage.Stage2_Dead, _ => RequiredCropAction.Water },
		{MushroomStage.Stage2_Growing, _ => RequiredCropAction.None },

		{MushroomStage.Stage3, _ => RequiredCropAction.SingleTap },
		{MushroomStage.Stage3_BeforeInoculation, _ => RequiredCropAction.Hold_2 },
		{MushroomStage.Stage3_AfterInoculation, _ => RequiredCropAction.None },

		{MushroomStage.Mature, _ => RequiredCropAction.TripleTap },
		{MushroomStage.Harvested, _ => RequiredCropAction.SingleTap },
	};
}

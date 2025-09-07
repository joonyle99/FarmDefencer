using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class CropSweetpotato : Crop
{
	[Serializable]
	private struct SweetpotatoState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float LastSingleTapTime { get; set; }
		public int TapCount { get; set; }
		public int RemainingSweetpotatoCount { get; set; } // 현재 남은 정상인 것의 개수
		public int InitialDeterminedSweetpotatoCount { get; set; } // 최초 결정된 썩은 것 + 정상인 것의 개수
		public bool Wrapped { get; set; }
		public float HoldingTime { get; set; }
		public bool DeterminedCount { get; set; }
		public float DecayRatio { get; set; }
	}

	private enum SweetpotatoStage
	{
		Unplowed,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,

		Stage3_BeforeWrap,
		Stage3_AfterWrap,

		Stage4,

		Mature,
		Harvested,
	}

	private const float Stage1_GrowthSeconds = 2.0f;
	private const float Stage2_GrowthSeconds = 2.0f;
	private const float Stage3_GrowthSeconds = 1.5f;
	private const float Stage4_GrowthSeconds = 1.5f;
	private const float WrapHoldingSecondsCriterion = 1.0f;

	[SerializeField] private Sprite stage1_beforeWaterSprite;
	[SerializeField] private Sprite stage1_deadSprite;
	[SerializeField] private Sprite stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite stage2_beforeWaterSprite;
	[SerializeField] private Sprite stage2_deadSprite;
	[SerializeField] private Sprite stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite stage3_beforeWrapSprite;
	[SerializeField] private Sprite stage3_afterWrapSprite;
	[Space]
	[SerializeField] private Sprite stage4Sprite;
	[Space]
	[SerializeField] private Sprite mature_X_sprite;
	[SerializeField] private Sprite mature_O_sprite;
	[Space]
	[SerializeField] private Sprite mature_OO_sprite;
	[SerializeField] private Sprite mature_XO_sprite;
	[SerializeField] private Sprite mature_XX_sprite;
	[Space]
	[SerializeField] private Sprite mature_XXX_sprite;
	[SerializeField] private Sprite mature_XXO_sprite;
	[SerializeField] private Sprite mature_XOO_sprite;
	[SerializeField] private Sprite mature_OOO_sprite;
	[Space]
	[SerializeField] private Sprite harvested_1_sprite;
	[SerializeField] private Sprite harvested_2_sprite;
	[SerializeField] private Sprite harvested_3_sprite;

	private SpriteRenderer _spriteRenderer;
	private SweetpotatoState _currentState;

	public bool ForceHarvestOne { get; set; }

	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);
	
	protected override int HarvestableCount => _currentState.Harvested ? (100 * _currentState.RemainingSweetpotatoCount) : 0;
	
	public override float? GaugeRatio =>
		GetCurrentStage(_currentState) is SweetpotatoStage.Mature or SweetpotatoStage.Harvested
			? 1.0f - _currentState.DecayRatio
			: null;

	public override void ApplyCommand(CropCommand cropCommand)
	{
		var currentStage = GetCurrentStage(_currentState);

		switch (cropCommand)
		{
			case GrowCommand when currentStage is SweetpotatoStage.Stage1_Growing:
			{
				_currentState.GrowthSeconds = Stage1_GrowthSeconds;
				break;
			}
			case GrowCommand when currentStage is SweetpotatoStage.Stage2_Growing:
			{
				_currentState.GrowthSeconds = Stage2_GrowthSeconds;
				break;
			}
			case WaterCommand when currentStage is SweetpotatoStage.Stage1_BeforeWater or SweetpotatoStage.Stage2_BeforeWater:
			{
				_currentState.Watered = true;
				break;
			}
		}
	}

	public override JObject Serialize() => JObject.FromObject(_currentState);

	public override void Deserialize(JObject json)
	{
		var state = JsonConvert.DeserializeObject<SweetpotatoState?>(json.ToString());
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
		var currentStage = GetCurrentStage(_currentState);
		_currentState = CommonCropBehavior(
			Effects,
			beforeState => OnHoldFunctions[currentStage](beforeState, initialPosition, deltaPosition, isEnd, deltaHoldTime),
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
		GetSpriteAndApplyTo_CountVarying(_currentState)(_spriteRenderer);

		_currentState = CommonCropBehavior(
			Effects,
			beforeState => OnFarmUpdateFunctions[currentStage](beforeState, deltaTime),
			_currentState,
			transform.position);
		
		if (ForceHarvestOne && _currentState.DeterminedCount)
		{
			_currentState.InitialDeterminedSweetpotatoCount = 1;
			_currentState.RemainingSweetpotatoCount = 1;
		}
	}

	public override void ResetToInitialState() => _currentState = ResetCropState(_currentState);

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// 이하 함수 빌딩 블록

	private static SweetpotatoStage GetCurrentStage(SweetpotatoState state) => state switch
	{
		{ Planted: false } => SweetpotatoStage.Unplowed,
		{ Harvested: true } => SweetpotatoStage.Harvested,
		{ DeterminedCount: true } => SweetpotatoStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds } => SweetpotatoStage.Stage4,

		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds, Wrapped: true } => SweetpotatoStage.Stage3_AfterWrap,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => SweetpotatoStage.Stage3_BeforeWrap,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => SweetpotatoStage.Unplowed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => SweetpotatoStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => SweetpotatoStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => SweetpotatoStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => SweetpotatoStage.Unplowed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => SweetpotatoStage.Stage1_Dead,
		{ Watered: true } => SweetpotatoStage.Stage1_Growing,
		{ } => SweetpotatoStage.Stage1_BeforeWater,
	};

	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo(SweetpotatoStage stage) => stage switch
	{
		SweetpotatoStage.Unplowed => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		SweetpotatoStage.Stage1_Dead => (spriteRenderer) => ApplySprite(stage1_deadSprite, spriteRenderer),
		SweetpotatoStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(stage1_beforeWaterSprite, spriteRenderer),
		SweetpotatoStage.Stage1_Growing => (spriteRenderer) => ApplySprite(stage1_growingSprite, spriteRenderer),

		SweetpotatoStage.Stage2_Dead => (spriteRenderer) => ApplySprite(stage2_deadSprite, spriteRenderer),
		SweetpotatoStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(stage2_beforeWaterSprite, spriteRenderer),
		SweetpotatoStage.Stage2_Growing => (spriteRenderer) => ApplySprite(stage2_growingSprite, spriteRenderer),

		SweetpotatoStage.Stage3_BeforeWrap => (spriteRenderer) => ApplySprite(stage3_beforeWrapSprite, spriteRenderer),
		SweetpotatoStage.Stage3_AfterWrap => (spriteRenderer) => ApplySprite(stage3_afterWrapSprite, spriteRenderer),

		SweetpotatoStage.Stage4 => (spriteRenderer) => ApplySprite(stage4Sprite, spriteRenderer),

		SweetpotatoStage.Mature => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		SweetpotatoStage.Harvested => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		_ => (_) => { }
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState, bool> HoldEffectCondition = (beforeState, afterState) => afterState.HoldingTime > beforeState.HoldingTime;
	private static readonly Action<Vector2, Vector2> HoldEffect = (inputWorldPosition, cropPosition) =>
	{
		EffectPlayer.SceneGlobalInstance.PlayHoldEffect(inputWorldPosition);
		EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilStone", cropPosition, false);
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState, bool> HoldStopEffectCondition = (beforeState, afterState) =>
	{
		return afterState.HoldingTime == 0.0f && beforeState.HoldingTime > 0.0f;
	};
	private static readonly Action<Vector2, Vector2> HoldStopEffect = (_, _) =>
	{
		EffectPlayer.SceneGlobalInstance.StopVfx();
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState, bool> TapEffectCondition = (beforeState, afterState) => afterState.LastSingleTapTime > beforeState.LastSingleTapTime;
	private static readonly Action<Vector2, Vector2> TapEffect = (inputWorldPosition, _) => EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);

	private static readonly Func<SweetpotatoState, SweetpotatoState, bool> WrapEffectCondition = (beforeState, afterState) => afterState.Wrapped && !beforeState.Wrapped;
	private static readonly Action<Vector2, Vector2> WrapEffect = (_, cropPosition) =>
	{
		EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDust", cropPosition);
		SoundManager.Instance.PlaySfx("SFX_T_sweet_vinyl", SoundManager.Instance.SweetPotatoVinylVolume);
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState, bool> SweetpotatoHarvestEffectCondition = (beforeState, afterState) => afterState.TapCount == 2 && beforeState.TapCount != 2;
	private static readonly Action<Vector2, Vector2> SweetpotatoHarvestEffect = (_, cropPosition) => EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDust", cropPosition);

	private static readonly List<(Func<SweetpotatoState, SweetpotatoState, bool>, Action<Vector2, Vector2>)> Effects = new()
	{
		(WrapEffectCondition, WrapEffect),
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HoldEffectCondition, HoldEffect),
		(HoldStopEffectCondition, HoldStopEffect),
		(TapEffectCondition, TapEffect),
		(SweetpotatoHarvestEffectCondition, SweetpotatoHarvestEffect),
	};

	/// <summary>
	/// Mature, Harvested단계에서 개수별로 다른 종류의 스프라이트를 적용하기 위한 함수.
	/// </summary>
	[Pure]
	private Action<SpriteRenderer> GetSpriteAndApplyTo_CountVarying(SweetpotatoState state) => state switch
	{
		{ DeterminedCount: false } => (_) => { },
		{ Harvested: true } =>
		(spriteRenderer)
		=>
		{
			if (state.RemainingSweetpotatoCount == 1)
			{
				ApplySprite(harvested_1_sprite, spriteRenderer);
			}
			else if (state.RemainingSweetpotatoCount == 2)
			{
				ApplySprite(harvested_2_sprite, spriteRenderer);
			}
			else
			{
				ApplySprite(harvested_3_sprite, spriteRenderer);
			}
		},
		{ GrowthSeconds: >= Stage1_GrowthSeconds+Stage2_GrowthSeconds+Stage3_GrowthSeconds+Stage4_GrowthSeconds} =>
		(spriteRenderer)
		=>
		{
			if (state.InitialDeterminedSweetpotatoCount == 1)
			{
				if (state.RemainingSweetpotatoCount == 1)
				{
					ApplySprite(mature_O_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(mature_X_sprite, spriteRenderer);
				}
			}
			else if (state.InitialDeterminedSweetpotatoCount == 2)
			{
				if (state.RemainingSweetpotatoCount == 0)
				{
					ApplySprite(mature_XX_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 1)
				{
					ApplySprite(mature_XO_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(mature_OO_sprite, spriteRenderer);
				}
			}
			else
			{
				if (state.RemainingSweetpotatoCount == 0)
				{
					ApplySprite(mature_XXX_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 1)
				{
					ApplySprite(mature_XXO_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 2)
				{
					ApplySprite(mature_XOO_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(mature_OOO_sprite, spriteRenderer);
				}
			}
		},
		_ => (_) => { }
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState> HarvestIfTripleTap =
		(beforeState) =>
		{
			var nextState = beforeState;
			var currentTime = Time.time;
			var lastSingleTapTime = beforeState.LastSingleTapTime;
			if (currentTime < lastSingleTapTime + MultipleTouchSecondsCriterion)
			{
				nextState.TapCount = beforeState.TapCount+1;
			}
			else
			{
				nextState.TapCount = 1;
			}
			nextState.LastSingleTapTime = currentTime;
			if (nextState.TapCount >= 3)
			{
				if (nextState.RemainingSweetpotatoCount == 0)
				{
					nextState = ResetCropState(nextState);
				}
				else
				{
					nextState.Harvested = true;
				}
			}

			return nextState;
		};

	private static readonly Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState> Wrap =
	(beforeState, _, _, isEnd, deltaHoldTime) =>
	{
		var nextState = beforeState;

		nextState.HoldingTime += deltaHoldTime;

		 if (nextState.HoldingTime >= WrapHoldingSecondsCriterion)
		{
			nextState.Wrapped = true;
		}

		if (isEnd || nextState.Wrapped)
		{
			nextState.HoldingTime = 0.0f;
		}

		return nextState;
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState> DetermineSweetpotatoCount =
		(beforeState) =>
		{
			var nextState = beforeState;
			nextState.DeterminedCount = true;

			var countDecision = UnityEngine.Random.Range(0.0f, 1.0f);

			if (countDecision >= 0.0f && countDecision < 0.6f)
			{
				nextState.InitialDeterminedSweetpotatoCount = 1;

				var rotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.2f;
				nextState.RemainingSweetpotatoCount = rotten ? 0 : 1;
			}
			else if (countDecision >= 0.6f && countDecision < 0.9f)
			{
				nextState.InitialDeterminedSweetpotatoCount = 2;

				var firstRotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.3f;
				var secondRotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.3f;

				if (firstRotten && secondRotten)
				{
					nextState.RemainingSweetpotatoCount = 0;
				}
				else if (!firstRotten && !secondRotten)
				{
					nextState.RemainingSweetpotatoCount = 2;
				}
				else
				{
					nextState.RemainingSweetpotatoCount = 1;
				}
			}
			else
			{
				nextState.InitialDeterminedSweetpotatoCount = 3;

				var firstRotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.4f;
				var secondRotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.4f;
				var thirdRotten = UnityEngine.Random.Range(0.0f, 1.0f) < 0.4f;

				if (firstRotten && secondRotten && thirdRotten)
				{
					nextState.RemainingSweetpotatoCount = 0;
				}
				else if (!firstRotten && !secondRotten && !thirdRotten)
				{
					nextState.RemainingSweetpotatoCount = 3;
				}
				else if (firstRotten && !secondRotten && !thirdRotten
				|| !firstRotten && secondRotten && !thirdRotten
				|| !firstRotten && !secondRotten && thirdRotten)
				{
					nextState.RemainingSweetpotatoCount = 1;
				}
				else
				{
					nextState.RemainingSweetpotatoCount = 2;
				}
			}

			return nextState;
		};

	private static readonly Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState> Plow =
		(beforeState, _, deltaPosition, isEnd, deltaHoldTime) =>
		{
			var nextState = beforeState;
			nextState.HoldingTime += deltaHoldTime;
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

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, float, SweetpotatoState>> OnFarmUpdateFunctions = new()
	{
		{
			SweetpotatoStage.Unplowed,
			(beforeState, deltaTime) =>
			{
				var holdTime = beforeState.HoldingTime;
				var reset = ResetCropState(beforeState);
				reset.HoldingTime = holdTime;
				return reset;
			}
		},

		{SweetpotatoStage.Stage1_Dead, WaitWater },
		{SweetpotatoStage.Stage1_BeforeWater, WaitWater },
		{
			SweetpotatoStage.Stage1_Growing,
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

		{SweetpotatoStage.Stage2_BeforeWater, WaitWater },
		{SweetpotatoStage.Stage2_Dead, WaitWater },
		{SweetpotatoStage.Stage2_Growing, Grow },

		{SweetpotatoStage.Stage3_BeforeWrap, DoNothing_OnFarmUpdate },
		{SweetpotatoStage.Stage3_AfterWrap, Grow },

		{
			SweetpotatoStage.Stage4,
			(beforeState, deltaTime) =>
			{
				var nextState = Grow(beforeState, deltaTime);
				if (nextState.GrowthSeconds >= Stage1_GrowthSeconds + Stage2_GrowthSeconds + Stage3_GrowthSeconds + Stage4_GrowthSeconds)
				{
					nextState = DetermineSweetpotatoCount(nextState);
				}
				return nextState;
			}
		},

		{SweetpotatoStage.Mature, Decay },
		{SweetpotatoStage.Harvested, DoNothing_OnFarmUpdate },
	};

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, SweetpotatoState>> OnTapFunctions = new()
	{
		{SweetpotatoStage.Unplowed, DoNothing },

		{SweetpotatoStage.Stage1_Dead, DoNothing },
		{SweetpotatoStage.Stage1_BeforeWater, DoNothing },
		{SweetpotatoStage.Stage1_Growing, DoNothing },

		{SweetpotatoStage.Stage2_BeforeWater, DoNothing },
		{SweetpotatoStage.Stage2_Dead, DoNothing },
		{SweetpotatoStage.Stage2_Growing, DoNothing },

		{SweetpotatoStage.Stage3_BeforeWrap, DoNothing },
		{SweetpotatoStage.Stage3_AfterWrap, DoNothing },

		{SweetpotatoStage.Stage4, DoNothing },

		{SweetpotatoStage.Mature, HarvestIfTripleTap },
		{SweetpotatoStage.Harvested, ResetCropState },
	};
	
	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, SweetpotatoState>> OnWateringFunctions = new()
	{
		{SweetpotatoStage.Unplowed, DoNothing },

		{SweetpotatoStage.Stage1_Dead, Water },
		{SweetpotatoStage.Stage1_BeforeWater, Water },
		{SweetpotatoStage.Stage1_Growing, DoNothing },

		{SweetpotatoStage.Stage2_BeforeWater, Water },
		{SweetpotatoStage.Stage2_Dead, Water },
		{SweetpotatoStage.Stage2_Growing, DoNothing },

		{SweetpotatoStage.Stage3_BeforeWrap, DoNothing },
		{SweetpotatoStage.Stage3_AfterWrap, DoNothing },

		{SweetpotatoStage.Stage4, DoNothing },

		{SweetpotatoStage.Mature, DoNothing },
		{SweetpotatoStage.Harvested, DoNothing },
	};
	
	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState>> OnHoldFunctions = new()
	{
		{SweetpotatoStage.Unplowed, Plow },

		{SweetpotatoStage.Stage1_Dead, DoNothing_OnHold },
		{SweetpotatoStage.Stage1_BeforeWater, DoNothing_OnHold },
		{SweetpotatoStage.Stage1_Growing, DoNothing_OnHold },

		{SweetpotatoStage.Stage2_BeforeWater, DoNothing_OnHold },
		{SweetpotatoStage.Stage2_Dead, DoNothing_OnHold },
		{SweetpotatoStage.Stage2_Growing, DoNothing_OnHold },

		{SweetpotatoStage.Stage3_BeforeWrap, Wrap },
		{SweetpotatoStage.Stage3_AfterWrap, DoNothing_OnHold },

		{SweetpotatoStage.Stage4, DoNothing_OnHold },

		{SweetpotatoStage.Mature, DoNothing_OnHold },
		{SweetpotatoStage.Harvested, DoNothing_OnHold },
	};

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{SweetpotatoStage.Unplowed, _ => RequiredCropAction.Drag },

		{SweetpotatoStage.Stage1_Dead, _ => RequiredCropAction.Water },
		{SweetpotatoStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
		{SweetpotatoStage.Stage1_Growing, _ => RequiredCropAction.None },

		{SweetpotatoStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
		{SweetpotatoStage.Stage2_Dead, _ => RequiredCropAction.Water },
		{SweetpotatoStage.Stage2_Growing, _ => RequiredCropAction.None },

		{SweetpotatoStage.Stage3_BeforeWrap, _ => RequiredCropAction.Hold_1 },
		{SweetpotatoStage.Stage3_AfterWrap, _ => RequiredCropAction.None },

		{SweetpotatoStage.Stage4, _ => RequiredCropAction.None },

		{SweetpotatoStage.Mature, _ => RequiredCropAction.TripleTap },
		{SweetpotatoStage.Harvested, _ => RequiredCropAction.SingleTap },
	};
}

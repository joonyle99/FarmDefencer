using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CropSweetpotato : Crop
{
	private struct SweetpotatoState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public int RemainingQuota { get; set; }
		public float LastSingleTapTime { get; set; }
		public int TapCount { get; set; }
		public int RemainingSweetpotatoCount { get; set; } // 현재 남은 정상인 것의 개수
		public int InitialDeterminedSweetpotatoCount { get; set; } // 최초 결정된 썩은 것 + 정상인 것의 개수
		public bool Wrapped { get; set; }
		public float HoldingTime { get; set; }
		public bool DeterminedCount { get; set; }
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

	private const float Stage1_GrowthSeconds = 15.0f;
	private const float Stage2_GrowthSeconds = 15.0f;
	private const float Stage3_GrowthSeconds = 10.0f;
	private const float Stage4_GrowthSeconds = 5.0f;
	private const float WrapHoldingSecondsCriterion = 2.0f;

	[SerializeField] private Sprite _stage1_beforeWaterSprite;
	[SerializeField] private Sprite _stage1_deadSprite;
	[SerializeField] private Sprite _stage1_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage2_beforeWaterSprite;
	[SerializeField] private Sprite _stage2_deadSprite;
	[SerializeField] private Sprite _stage2_growingSprite;
	[Space]
	[SerializeField] private Sprite _stage3_beforeWrapSprite;
	[SerializeField] private Sprite _stage3_afterWrapSprite;
	[Space]
	[SerializeField] private Sprite _stage4Sprite;
	[Space]
	[SerializeField] private Sprite _mature_X_sprite;
	[SerializeField] private Sprite _mature_O_sprite;
	[Space]
	[SerializeField] private Sprite _mature_OO_sprite;
	[SerializeField] private Sprite _mature_XO_sprite;
	[SerializeField] private Sprite _mature_XX_sprite;
	[Space]
	[SerializeField] private Sprite _mature_XXX_sprite;
	[SerializeField] private Sprite _mature_XXO_sprite;
	[SerializeField] private Sprite _mature_XOO_sprite;
	[SerializeField] private Sprite _mature_OOO_sprite;
	[Space]
	[SerializeField] private Sprite _harvested_1_sprite;
	[SerializeField] private Sprite _harvested_2_sprite;
	[SerializeField] private Sprite _harvested_3_sprite;

	private SpriteRenderer _spriteRenderer;
	private SweetpotatoState _currentState;

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
		var currentStage = GetCurrentStage(_currentState);
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState) =>
			{
				return OnSingleHoldingFunctions[currentStage](beforeState, initialPosition, deltaPosition, isEnd, deltaHoldTime);
			},
			_currentState)

			(initialPosition+deltaPosition, transform.position);
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
		GetSpriteAndApplyTo_CountVarying(_currentState)(_spriteRenderer);

		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			(beforeState) =>
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

		SweetpotatoStage.Stage1_Dead => (spriteRenderer) => ApplySprite(_stage1_deadSprite, spriteRenderer),
		SweetpotatoStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(_stage1_beforeWaterSprite, spriteRenderer),
		SweetpotatoStage.Stage1_Growing => (spriteRenderer) => ApplySprite(_stage1_growingSprite, spriteRenderer),

		SweetpotatoStage.Stage2_Dead => (spriteRenderer) => ApplySprite(_stage2_deadSprite, spriteRenderer),
		SweetpotatoStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(_stage2_beforeWaterSprite, spriteRenderer),
		SweetpotatoStage.Stage2_Growing => (spriteRenderer) => ApplySprite(_stage2_growingSprite, spriteRenderer),

		SweetpotatoStage.Stage3_BeforeWrap => (spriteRenderer) => ApplySprite(_stage3_beforeWrapSprite, spriteRenderer),
		SweetpotatoStage.Stage3_AfterWrap => (spriteRenderer) => ApplySprite(_stage3_afterWrapSprite, spriteRenderer),

		SweetpotatoStage.Stage4 => (spriteRenderer) => ApplySprite(_stage4Sprite, spriteRenderer),

		SweetpotatoStage.Mature => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		SweetpotatoStage.Harvested => (spriteRenderer) => ApplySprite(null, spriteRenderer),

		_ => (_) => { }
	};

	private static readonly List<(Func<SweetpotatoState, SweetpotatoState, bool>, Action<Vector2, Vector2>)> Effects = new List<(Func<SweetpotatoState, SweetpotatoState, bool>, Action<Vector2, Vector2>)>
	{
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(HarvestEffectCondition, HarvestEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
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
				ApplySprite(_harvested_1_sprite, spriteRenderer);
			}
			else if (state.RemainingSweetpotatoCount == 2)
			{
				ApplySprite(_harvested_2_sprite, spriteRenderer);
			}
			else
			{
				ApplySprite(_harvested_3_sprite, spriteRenderer);
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
					ApplySprite(_mature_O_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(_mature_X_sprite, spriteRenderer);
				}
			}
			else if (state.InitialDeterminedSweetpotatoCount == 2)
			{
				if (state.RemainingSweetpotatoCount == 0)
				{
					ApplySprite(_mature_XX_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 1)
				{
					ApplySprite(_mature_XO_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(_mature_OO_sprite, spriteRenderer);
				}
			}
			else
			{
				if (state.RemainingSweetpotatoCount == 0)
				{
					ApplySprite(_mature_XXX_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 1)
				{
					ApplySprite(_mature_XXO_sprite, spriteRenderer);
				}
				else if (state.RemainingSweetpotatoCount == 2)
				{
					ApplySprite(_mature_XOO_sprite, spriteRenderer);
				}
				else
				{
					ApplySprite(_mature_OOO_sprite, spriteRenderer);
				}
			}
		},
		_ => (_) => { }
	};

	private static readonly Func<SweetpotatoState, SweetpotatoState> HarvestIfFiveTap =
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
			if (nextState.TapCount >= 5)
			{
				nextState.Harvested = true;
			}

			return nextState;
		};

	private static readonly Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState> Wrap =
	(beforeState, initialWorldPosition, deltaPosition, isEnd, deltaHoldTime) =>
	{
		var nextState = beforeState;

		nextState.HoldingTime += deltaHoldTime;

		 if (nextState.HoldingTime >= WrapHoldingSecondsCriterion)
		{
			nextState.Wrapped = true;
		}

		if (isEnd)
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
		(beforeState, initialWorldPosition, deltaPosition, isEnd, deltaHoldTime) =>
		{
			var nextState = beforeState;
			if (deltaPosition.x >= PlowDeltaPositionCrierion)
			{
				nextState.Planted = true;
			}

			return nextState;
		};

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, float, SweetpotatoState>> OnFarmUpdateFunctions = new Dictionary<SweetpotatoStage, Func<SweetpotatoState, float, SweetpotatoState>>
	{
		{SweetpotatoStage.Unplowed, (beforeState, deltaTime) => Reset(beforeState) },

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

		{SweetpotatoStage.Mature, DoNothing_OnFarmUpdate },
		{SweetpotatoStage.Harvested, DoNothing_OnFarmUpdate },

	};

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, SweetpotatoState>> OnSingleTapFunctions = new Dictionary<SweetpotatoStage, Func<SweetpotatoState, SweetpotatoState>>
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

		{SweetpotatoStage.Mature, HarvestIfFiveTap },
		{SweetpotatoStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, beforeState.RemainingSweetpotatoCount) },
	};

	private static readonly Dictionary<SweetpotatoStage, Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState>> OnSingleHoldingFunctions = new Dictionary<SweetpotatoStage, Func<SweetpotatoState, Vector2, Vector2, bool, float, SweetpotatoState>>
	{
		{SweetpotatoStage.Unplowed, Plow },

		{SweetpotatoStage.Stage1_Dead, DoNothing_OnSingleHolding },
		{SweetpotatoStage.Stage1_BeforeWater, DoNothing_OnSingleHolding },
		{SweetpotatoStage.Stage1_Growing, DoNothing_OnSingleHolding },

		{SweetpotatoStage.Stage2_BeforeWater, DoNothing_OnSingleHolding },
		{SweetpotatoStage.Stage2_Dead, DoNothing_OnSingleHolding },
		{SweetpotatoStage.Stage2_Growing, DoNothing_OnSingleHolding },

		{SweetpotatoStage.Stage3_BeforeWrap, Wrap },
		{SweetpotatoStage.Stage3_AfterWrap, DoNothing_OnSingleHolding },

		{SweetpotatoStage.Stage4, DoNothing_OnSingleHolding },

		{SweetpotatoStage.Mature, DoNothing_OnSingleHolding },
		{SweetpotatoStage.Harvested, DoNothing_OnSingleHolding },
	};
}

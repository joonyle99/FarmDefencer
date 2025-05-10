using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 0번 자식에 수확 단계 흙을 표시하기 위한 SpriteRenderer 오브젝트를 할당하고,
/// 1번 자식에 수확 단계 작물을 표시하기 위한 SpriteRenderer 오브젝트를 할당할 것.
/// </summary>
public sealed class CropCabbage : Crop
{
	private struct CabbageState : ICommonCropState
	{
		public bool Planted { get; set; }
		public float WaterWaitingSeconds { get; set; }
		public float GrowthSeconds { get; set; }
		public bool Watered { get; set; }
		public bool Harvested { get; set; }
		public float HoldingTime { get; set; }
		public int RemainingQuota { get; set; }
		public float Shake { get; set; }
		public int ShakeCount { get; set; }
		public bool WasLastShakeLeftSide { get; set; }
	}

	private enum CabbageStage
	{
		Seed,
		Mature,
		Harvested,

		Stage1_BeforeWater,
		Stage1_Dead,
		Stage1_Growing,

		Stage2_BeforeWater,
		Stage2_Dead,
		Stage2_Growing,
	}

	private const float DeltaShakeCriterion = 0.25f; // 배추 수확단계 흔들기 기준 (가로 방향 위치 델타)
	private const int ShakeCountCriterion = 4; // 배추 수확 흔들기 횟수 기준
	private const float Stage1_GrowthSeconds = 10.0f;
	private const float Stage2_GrowthSeconds = 10.0f;

	[SerializeField] private Sprite seedSprite;
	[SerializeField] private Sprite harvestedSprite;

	[SerializeField] private Sprite stage1_beforeWaterSprite;
	[SerializeField] private Sprite stage1_deadSprite;
	[SerializeField] private Sprite stage1_growingSprite;

	[SerializeField] private Sprite stage2_beforeWaterSprite;
	[SerializeField] private Sprite stage2_deadSprite;
	[SerializeField] private Sprite stage2_growingSprite;

	private SpriteRenderer _spriteRenderer; // 수확 단계를 제외한 모든 단계의 스프라이트 표시
	private CabbageState _currentState;
	private GameObject _harvestSoilLayerObject; // 0번 자식
	private GameObject _harvestCropLayerObject; // 1번 자식
	
	public override RequiredCropAction RequiredCropAction =>
		GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);

	public override void OnSingleTap(Vector2 worldPosition)
	{
		_currentState = HandleAction_NotifyFilledQuota_PlayEffectAt(

			Effects,
			GetQuota,
			NotifyQuotaFilled,
			OnSingleTapFunctions[GetCurrentStage(_currentState)], _currentState)

			(worldPosition, transform.position);
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

	public override void OnFarmUpdate(float deltaTime)
	{
		var currentStage = GetCurrentStage(_currentState);
		ApplySpriteTo(currentStage)(_spriteRenderer);

		if (currentStage == CabbageStage.Mature)
		{
			_harvestSoilLayerObject.SetActive(true);
			_harvestCropLayerObject.SetActive(true);
			_harvestCropLayerObject.transform.localEulerAngles = _harvestCropLayerObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Atan(-_currentState.Shake) * 90.0f / Mathf.PI);
		}
		else
		{
			_harvestSoilLayerObject.SetActive(false);
			_harvestCropLayerObject.SetActive(false);
		}

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
		_harvestSoilLayerObject = transform.GetChild(0).gameObject;
		_harvestCropLayerObject = transform.GetChild(1).gameObject;
	}

	// 이하 함수 빌딩 블록

	[Pure]
	private Action<SpriteRenderer> ApplySpriteTo(CabbageStage stage) => stage switch
	{
		CabbageStage.Seed when _spriteRenderer.sprite != seedSprite => (spriteRenderer) => spriteRenderer.sprite = seedSprite,
		CabbageStage.Mature when _spriteRenderer.sprite != null => (spriteRenderer) => spriteRenderer.sprite = null,
		CabbageStage.Harvested when _spriteRenderer.sprite != harvestedSprite => (spriteRenderer) => spriteRenderer.sprite = harvestedSprite,

		CabbageStage.Stage2_Dead when _spriteRenderer.sprite != stage2_deadSprite => (spriteRenderer) => spriteRenderer.sprite = stage2_deadSprite,
		CabbageStage.Stage2_BeforeWater when _spriteRenderer.sprite != stage2_beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = stage2_beforeWaterSprite,
		CabbageStage.Stage2_Growing when _spriteRenderer.sprite != stage2_growingSprite => (spriteRenderer) => spriteRenderer.sprite = stage2_growingSprite,

		CabbageStage.Stage1_Dead when _spriteRenderer.sprite != stage1_deadSprite => (spriteRenderer) => spriteRenderer.sprite = stage1_deadSprite,
		CabbageStage.Stage1_BeforeWater when _spriteRenderer.sprite != stage1_beforeWaterSprite => (spriteRenderer) => spriteRenderer.sprite = stage1_beforeWaterSprite,
		CabbageStage.Stage1_Growing when _spriteRenderer.sprite != stage1_growingSprite => (spriteRenderer) => spriteRenderer.sprite = stage1_growingSprite,

		_ => (_) => { }
	};

	private static readonly Func<CabbageState, CabbageState, bool> ShakeEffectCondition = (beforeState, afterState) => afterState.ShakeCount > beforeState.ShakeCount;
	private static readonly Action<Vector2, Vector2> ShakeEffect = (_, cropPosition) =>
	{
		EffectPlayer.PlayVfx("VFX_T_SoilParticle", cropPosition);
	};

	private static readonly Func<int, Func<CabbageState, CabbageState, bool>> ShakeSfxEffectConditionFor = shakedCount => (beforeState, afterState) => afterState.ShakeCount == shakedCount && beforeState.ShakeCount < shakedCount;
	private static readonly Func<int, Action<Vector2, Vector2>> ShakeSfxEffectFor = shakedCount => (_, _) => SoundManager.PlaySfxStatic($"SFX_T_cabbage_shake_{shakedCount}");

	private static List<(Func<CabbageState, CabbageState, bool>, Action<Vector2, Vector2>)> Effects = new()
	{
		(HarvestEffectCondition, HarvestEffect_SoilParticle),
		(WaterEffectCondition, WaterEffect),
		(PlantEffectCondition, PlantEffect),
		(QuotaFilledEffectCondition, QuotaFilledEffect),
		(ShakeEffectCondition, ShakeEffect),
		(ShakeSfxEffectConditionFor(1), ShakeSfxEffectFor(1)),
		(ShakeSfxEffectConditionFor(2), ShakeSfxEffectFor(2)),
		(ShakeSfxEffectConditionFor(3), ShakeSfxEffectFor(3)),
		(ShakeSfxEffectConditionFor(4), ShakeSfxEffectFor(4)),
	};

	private static readonly Func<CabbageState, Vector2, Vector2, bool, float, CabbageState> ShakeAndHarvest =
		(beforeState, _, deltaPosition, isEnd, _)
		=>
		{
			var nextState = beforeState;
			nextState.Shake = deltaPosition.x;

			if (isEnd)
			{
				nextState.Shake = 0.0f;
				nextState.ShakeCount = 0;
			}

			if (Math.Abs(deltaPosition.x) > DeltaShakeCriterion)
			{
				if (beforeState.ShakeCount == 0)
				{
					nextState.ShakeCount += 1;
					nextState.WasLastShakeLeftSide = deltaPosition.x < 0.0f;
				}
				else
				{
					if (beforeState.WasLastShakeLeftSide && deltaPosition.x > 0.0f
					|| !beforeState.WasLastShakeLeftSide && deltaPosition.x < 0.0f)
					{
						nextState.ShakeCount += 1;
						nextState.WasLastShakeLeftSide = !beforeState.WasLastShakeLeftSide;
					}
				}

				if (nextState.ShakeCount >= ShakeCountCriterion)
				{
					nextState.Harvested = true;
				}
			}


			return nextState;
		};

	private static CabbageStage GetCurrentStage(CabbageState state) => state switch
	{
		{ Planted: false } => CabbageStage.Seed,
		{ Harvested: true } => CabbageStage.Harvested,
		{ GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => CabbageStage.Mature,

		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CabbageStage.Seed,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CabbageStage.Stage2_Dead,
		{ GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => CabbageStage.Stage2_Growing,
		{ GrowthSeconds: >= Stage1_GrowthSeconds } => CabbageStage.Stage2_BeforeWater,

		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CabbageStage.Seed,
		{ WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CabbageStage.Stage1_Dead,
		{ Watered: true } => CabbageStage.Stage1_Growing,
		{ } => CabbageStage.Stage1_BeforeWater,
	};

	private static readonly Dictionary<CabbageStage, Func<CabbageState, float, CabbageState>> OnFarmUpdateFunctions = new()
	{
		{CabbageStage.Seed, (currentState, _) => Reset(currentState) },
		{CabbageStage.Mature, (currentState, _) => DoNothing(currentState) },
		{CabbageStage.Harvested, (currentState, _) => DoNothing(currentState) },

		{CabbageStage.Stage2_Dead, WaitWater },
		{CabbageStage.Stage2_BeforeWater, WaitWater },
		{CabbageStage.Stage2_Growing, Grow },

		{CabbageStage.Stage1_Dead, WaitWater },
		{CabbageStage.Stage1_BeforeWater, WaitWater },
		{
			CabbageStage.Stage1_Growing,
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
	};

	private static readonly Dictionary<CabbageStage, Func<CabbageState, CabbageState>> OnSingleTapFunctions = new()
	{
		{CabbageStage.Seed, Plant },
		{CabbageStage.Harvested, (beforeState) => FillQuotaUptoAndResetIfEqual(beforeState, 1) },
		{CabbageStage.Mature, DoNothing },
		
		{CabbageStage.Stage2_Dead, DoNothing },
		{CabbageStage.Stage2_BeforeWater, DoNothing },
		{CabbageStage.Stage2_Growing, DoNothing },
		
		{CabbageStage.Stage1_Dead, DoNothing },
		{CabbageStage.Stage1_BeforeWater, DoNothing },
		{CabbageStage.Stage1_Growing, DoNothing },
	};


	private static readonly Dictionary<CabbageStage, Func<CabbageState, Vector2, Vector2, bool, float, CabbageState>> OnSingleHoldingFunctions = new Dictionary<CabbageStage, Func<CabbageState, Vector2, Vector2, bool, float, CabbageState>>
	{
		{CabbageStage.Seed, DoNothing_OnSingleHolding },
		{CabbageStage.Harvested, DoNothing_OnSingleHolding },
		{CabbageStage.Mature, ShakeAndHarvest },
		
		{CabbageStage.Stage2_BeforeWater, DoNothing_OnSingleHolding },
		{CabbageStage.Stage2_Dead, DoNothing_OnSingleHolding },
		{CabbageStage.Stage2_Growing, DoNothing_OnSingleHolding },
		
		{CabbageStage.Stage1_BeforeWater, DoNothing_OnSingleHolding },
		{CabbageStage.Stage1_Dead, DoNothing_OnSingleHolding },
		{CabbageStage.Stage1_Growing, DoNothing_OnSingleHolding },
	};
	
	private static readonly Dictionary<CabbageStage, Func<CabbageState, RequiredCropAction>> GetRequiredCropActionFunctions = new()
	{
		{CabbageStage.Seed, _ => RequiredCropAction.SingleTap },
		{CabbageStage.Harvested, _ => RequiredCropAction.SingleTap },
		{CabbageStage.Mature, _ => RequiredCropAction.Drag },
		
		{CabbageStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
		{CabbageStage.Stage2_Dead, _ => RequiredCropAction.Water },
		{CabbageStage.Stage2_Growing, _ => RequiredCropAction.None },
		
		{CabbageStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
		{CabbageStage.Stage1_Dead, _ => RequiredCropAction.Water },
		{CabbageStage.Stage1_Growing, _ => RequiredCropAction.None },
	};
}

using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

public sealed class CropCucumber : Crop
{
    [Serializable]
    private struct CucumberState : ICommonCropState
    {
        public bool Planted { get; set; }
        public float WaterWaitingSeconds { get; set; }
        public float GrowthSeconds { get; set; }
        public bool Watered { get; set; }
        public bool Harvested { get; set; }
        public int RemainingQuota { get; set; }
        public bool ShortTrellisPlaced { get; set; }
        public bool LongTrellisPlaced { get; set; }
        public float DecayRatio { get; set; }
    }

    private enum CucumberStage
    {
        Seed,
        Mature,
        Harvested,

        Stage1_BeforeWater,
        Stage1_Dead,
        Stage1_Growing,

        Stage2_BeforeShortTrellis,
        Stage2_BeforeWater,
        Stage2_Dead,
        Stage2_Growing,

        Stage3
    }

    private const float Stage1_GrowthSeconds = 15.0f;
    private const float Stage2_GrowthSeconds = 15.0f;

    [SerializeField] private Sprite seedSprite;
    [Space] [SerializeField] private Sprite stage1_beforeWaterSprite;
    [SerializeField] private Sprite stage1_deadSprite;
    [SerializeField] private Sprite stage1_growingSprite;

    [FormerlySerializedAs("stage2_beforeShorttrelisSprite")] [Space] [SerializeField]
    private Sprite stage2_beforeShortTrellisSprite;

    [SerializeField] private Sprite stage2_beforeWaterSprite;
    [SerializeField] private Sprite stage2_deadSprite;
    [SerializeField] private Sprite stage2_growingSprite;
    [Space] [SerializeField] private Sprite stage3_sprite;
    [Space] [SerializeField] private Sprite matureSprite;
    [SerializeField] private Sprite harvestedSprite;

    private SpriteRenderer _spriteRenderer;
    private CucumberState _currentState;

    public override RequiredCropAction RequiredCropAction =>
        GetRequiredCropActionFunctions[GetCurrentStage(_currentState)](_currentState);

    public override float? GaugeRatio =>
        GetCurrentStage(_currentState) is CucumberStage.Mature or CucumberStage.Harvested
            ? 1.0f - _currentState.DecayRatio
            : null;

    public override void ApplyCommand(CropCommand cropCommand)
    {
        var currentStage = GetCurrentStage(_currentState);

        switch (cropCommand)
        {
            case GrowCommand when currentStage is CucumberStage.Stage1_Growing:
            {
                _currentState.GrowthSeconds = Stage1_GrowthSeconds;
                break;
            }
            case GrowCommand when currentStage is CucumberStage.Stage2_Growing:
            {
                _currentState.GrowthSeconds = Stage2_GrowthSeconds;
                break;
            }
            case WaterCommand when currentStage is CucumberStage.Stage1_BeforeWater or CucumberStage.Stage2_BeforeWater:
            {
                _currentState.Watered = true;
                break;
            }
        }
    }

    public override JObject Serialize() => JObject.FromObject(_currentState);

    public override void Deserialize(JObject json)
    {
        var state = JsonConvert.DeserializeObject<CucumberState?>(json.ToString());
        if (state != null)
        {
            _currentState = state.Value;
        }
    }

    public override void OnTap(Vector2 worldPosition)
    {
        _currentState = CommonCropBehavior(
            Effects,
            OnPlanted,
            OnSold,
            OnTapFunctions[GetCurrentStage(_currentState)],
            _currentState,
            worldPosition,
            transform.position);
    }

    public override bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime)
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
            OnWateringFunctions[GetCurrentStage(_currentState)],
            _currentState,
            transform.position,
            transform.position);
    }

    public override void OnFarmUpdate(float deltaTime)
    {
        var currentStage = GetCurrentStage(_currentState);
        GetSpriteAndApplyTo(currentStage)(_spriteRenderer);
        _currentState = CommonCropBehavior(
            Effects,
            OnPlanted,
            OnSold,
            beforeState => OnFarmUpdateFunctions[currentStage](beforeState, deltaTime),
            _currentState,
            transform.position,
            transform.position);
    }

    public override void ResetToInitialState() => _currentState = ResetCropState(_currentState);

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    // 이하 함수 빌딩 블록

    private static CucumberStage GetCurrentStage(CucumberState state) => state switch
    {
        { Planted: false } => CucumberStage.Seed,
        { Harvested: true } => CucumberStage.Harvested,
        { LongTrellisPlaced: true } => CucumberStage.Mature,

        { GrowthSeconds: >= Stage1_GrowthSeconds + Stage2_GrowthSeconds } => CucumberStage.Stage3,

        { GrowthSeconds: >= Stage1_GrowthSeconds, ShortTrellisPlaced: false } =>
            CucumberStage.Stage2_BeforeShortTrellis,
        {
            GrowthSeconds: >= Stage1_GrowthSeconds,
            WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds
        } => CucumberStage.Seed,
        { GrowthSeconds: >= Stage1_GrowthSeconds, WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CucumberStage
            .Stage2_Dead,
        { GrowthSeconds: >= Stage1_GrowthSeconds, Watered: true } => CucumberStage.Stage2_Growing,
        { GrowthSeconds: >= Stage1_GrowthSeconds } => CucumberStage.Stage2_BeforeWater,

        { WaterWaitingSeconds: >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds } => CucumberStage.Seed,
        { WaterWaitingSeconds: >= WaterWaitingDeadSeconds } => CucumberStage.Stage1_Dead,
        { Watered: true } => CucumberStage.Stage1_Growing,
        { } => CucumberStage.Stage1_BeforeWater,
    };

    private static readonly Dictionary<CucumberStage, Func<CucumberState, float, CucumberState>> OnFarmUpdateFunctions =
        new()
        {
            { CucumberStage.Seed, (beforeState, _) => ResetCropState(beforeState) },

            { CucumberStage.Stage1_Dead, WaitWater },
            { CucumberStage.Stage1_BeforeWater, WaitWater },
            {
                CucumberStage.Stage1_Growing,
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

            { CucumberStage.Stage2_BeforeShortTrellis, DoNothing_OnFarmUpdate },
            { CucumberStage.Stage2_Dead, WaitWater },
            { CucumberStage.Stage2_BeforeWater, WaitWater },
            { CucumberStage.Stage2_Growing, Grow },

            { CucumberStage.Stage3, DoNothing_OnFarmUpdate },

            { CucumberStage.Mature, Decay },
            { CucumberStage.Harvested, DoNothing_OnFarmUpdate },
        };

    private static readonly Dictionary<CucumberStage, Func<CucumberState, CucumberState>> OnTapFunctions = new()
    {
        { CucumberStage.Seed, DoNothing },

        { CucumberStage.Stage1_Dead, DoNothing },
        { CucumberStage.Stage1_BeforeWater, DoNothing },
        { CucumberStage.Stage1_Growing, DoNothing },

        {
            CucumberStage.Stage2_BeforeShortTrellis, (beforeState) =>
            {
                beforeState.ShortTrellisPlaced = true;
                return beforeState;
            }
        },
        { CucumberStage.Stage2_Dead, DoNothing },
        { CucumberStage.Stage2_BeforeWater, DoNothing },
        { CucumberStage.Stage2_Growing, DoNothing },

        {
            CucumberStage.Stage3, (beforeState) =>
            {
                beforeState.LongTrellisPlaced = true;
                return beforeState;
            }
        },

        { CucumberStage.Mature, DoNothing },
        { CucumberStage.Harvested, ResetCropState },
    };

    private static readonly Dictionary<CucumberStage, Func<CucumberState, CucumberState>> OnHoldFunctions = new()
    {
        { CucumberStage.Seed, Plant },

        { CucumberStage.Stage1_Dead, DoNothing },
        { CucumberStage.Stage1_BeforeWater, DoNothing },
        { CucumberStage.Stage1_Growing, DoNothing },

        { CucumberStage.Stage2_BeforeShortTrellis, DoNothing },
        { CucumberStage.Stage2_Dead, DoNothing },
        { CucumberStage.Stage2_BeforeWater, DoNothing },
        { CucumberStage.Stage2_Growing, DoNothing },

        { CucumberStage.Stage3, DoNothing },

        { CucumberStage.Mature, Harvest },
        { CucumberStage.Harvested, DoNothing },
    };

    private static readonly Dictionary<CucumberStage, Func<CucumberState, CucumberState>> OnWateringFunctions = new()
    {
        { CucumberStage.Seed, DoNothing },

        { CucumberStage.Stage1_Dead, Water },
        { CucumberStage.Stage1_BeforeWater, Water },
        { CucumberStage.Stage1_Growing, DoNothing },

        { CucumberStage.Stage2_BeforeShortTrellis, DoNothing },
        { CucumberStage.Stage2_Dead, Water },
        { CucumberStage.Stage2_BeforeWater, Water },
        { CucumberStage.Stage2_Growing, DoNothing },

        { CucumberStage.Stage3, DoNothing },

        { CucumberStage.Mature, DoNothing },
        { CucumberStage.Harvested, DoNothing },
    };

    private static readonly Dictionary<CucumberStage, Func<CucumberState, RequiredCropAction>>
        GetRequiredCropActionFunctions = new()
        {
            { CucumberStage.Seed, _ => RequiredCropAction.SingleTap },

            { CucumberStage.Stage1_Dead, _ => RequiredCropAction.Water },
            { CucumberStage.Stage1_BeforeWater, _ => RequiredCropAction.Water },
            { CucumberStage.Stage1_Growing, _ => RequiredCropAction.None },

            { CucumberStage.Stage2_BeforeShortTrellis, _ => RequiredCropAction.SingleTap },
            { CucumberStage.Stage2_Dead, _ => RequiredCropAction.Water },
            { CucumberStage.Stage2_BeforeWater, _ => RequiredCropAction.Water },
            { CucumberStage.Stage2_Growing, _ => RequiredCropAction.None },

            { CucumberStage.Stage3, _ => RequiredCropAction.SingleTap },

            { CucumberStage.Mature, _ => RequiredCropAction.SingleTap },
            { CucumberStage.Harvested, _ => RequiredCropAction.SingleTap },
        };

    [Pure]
    private Action<SpriteRenderer> GetSpriteAndApplyTo(CucumberStage cucumberStage) => cucumberStage switch
    {
        CucumberStage.Seed => (spriteRenderer) => ApplySprite(seedSprite, spriteRenderer),

        CucumberStage.Stage1_Dead => (spriteRenderer) => ApplySprite(stage1_deadSprite, spriteRenderer),
        CucumberStage.Stage1_BeforeWater => (spriteRenderer) => ApplySprite(stage1_beforeWaterSprite, spriteRenderer),
        CucumberStage.Stage1_Growing => (spriteRenderer) => ApplySprite(stage1_growingSprite, spriteRenderer),

        CucumberStage.Stage2_BeforeShortTrellis => (spriteRenderer) =>
            ApplySprite(stage2_beforeShortTrellisSprite, spriteRenderer),
        CucumberStage.Stage2_Dead => (spriteRenderer) => ApplySprite(stage2_deadSprite, spriteRenderer),
        CucumberStage.Stage2_BeforeWater => (spriteRenderer) => ApplySprite(stage2_beforeWaterSprite, spriteRenderer),
        CucumberStage.Stage2_Growing => (spriteRenderer) => ApplySprite(stage2_growingSprite, spriteRenderer),

        CucumberStage.Stage3 => (spriteRenderer) => ApplySprite(stage3_sprite, spriteRenderer),

        CucumberStage.Mature => (spriteRenderer) => ApplySprite(matureSprite, spriteRenderer),
        CucumberStage.Harvested => (spriteRenderer) => ApplySprite(harvestedSprite, spriteRenderer),
        _ => (_) => { }
    };

    private static readonly Func<CucumberState, CucumberState, bool> TrellisEffectCondition =
        (beforeState, afterState) => afterState.LongTrellisPlaced && !beforeState.LongTrellisPlaced ||
                                     afterState.ShortTrellisPlaced && !beforeState.ShortTrellisPlaced;

    private static readonly Action<Vector2, Vector2> TrellisEffect = (inputWorldPosition, cropPosition) =>
    {
        EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);
        EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilParticleWhite", cropPosition);
    };

    private static List<(Func<CucumberState, CucumberState, bool>, Action<Vector2, Vector2>)> Effects = new()
    {
        (WaterEffectCondition, WaterEffect),
        (PlantEffectCondition, PlantEffect),
        (HarvestEffectCondition, HarvestEffect_SoilDust),
        (TrellisEffectCondition, TrellisEffect),
    };
}
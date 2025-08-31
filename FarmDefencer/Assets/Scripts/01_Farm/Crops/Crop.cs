using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public abstract class CropCommand
{
}

public sealed class WaterCommand : CropCommand
{
}

public sealed class GrowCommand : CropCommand
{
}

public enum RequiredCropAction
{
    None,
    SingleTap,
    Hold_0_75,
    Hold_1,
    Hold_2,
    DoubleTap,
    FiveTap,
    Drag,
    Water,
}

[Serializable]
public abstract class Crop : MonoBehaviour, IFarmUpdatable, IFarmSerializable
{
    /// <summary>
    /// 필드는 반드시 초기값이 초기 상태를 의미하도록 되어야 한다.<br/>
    /// 예: { IsSeed = true }가 초기 상태 - X<br/>
    /// { IsPlanted = false } 가 초기 상태 - O<br/>
    /// 즉 컴파일러가 생성하는 초기값이 초기 상태여야 함.
    /// </summary>
    protected interface ICommonCropState
    {
        bool Planted { get; set; }
        bool Watered { get; set; }
        float WaterWaitingSeconds { get; set; }
        float GrowthSeconds { get; set; }
        bool Harvested { get; set; }
        float DecayRatio { get; set; }
    }

    protected const float WaterWaitingDeadSeconds = 90.0f;
    protected const float WaterWaitingResetSeconds = 60.0f;
    protected const float MultipleTouchSecondsCriterion = 0.5f; // 연속 탭 동작 판정 시간. 이 시간 이내로 다시 탭 해야 연속 탭으로 간주됨
    protected const float PlowDeltaPositionCriterion = 0.25f; // 밭 문지르기 동작 판정 기준 (가로 방향 위치 델타)

    public abstract RequiredCropAction RequiredCropAction { get; }

    public abstract float? GaugeRatio { get; }

    protected Action OnPlanted { get; private set; }

    protected Action<int> OnSold { get; private set; }

    public abstract JObject Serialize();

    public abstract void Deserialize(JObject json);

    public void Init(Action onPlanted, Action<int> onSold)
    {
        OnPlanted = onPlanted;
        OnSold = onSold;
    }

    public bool AABB(Vector2 worldPosition) =>
        Mathf.Abs(worldPosition.x - transform.position.x) < 0.5f &&
        Mathf.Abs(worldPosition.y - transform.position.y) < 0.5f;

    public abstract void ApplyCommand(CropCommand cropCommand);

    /// <summary>
    /// 한 손가락으로 탭할 때의 동작을 정의.
    /// </summary>
    /// <param name="inputWorldPosition"></param>
    public virtual void OnTap(Vector2 inputWorldPosition)
    {
    }

    /// <summary>
    /// 한 손가락으로 꾹 누를 때의 동작을 정의.
    /// </summary>
    /// <param name="initialWorldPosition">첫 홀드 위치.</param>
    /// <param name="deltaWorldPosition">첫 홀드 위치와 현재 홀드 위치의 차이.</param>
    /// <param name="isEnd">홀드가 종료되어 마지막 액션인 경우 true.</param>
    /// <param name="deltaHoldTime"></param>
    public virtual bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd,
        float deltaHoldTime) => false;

    public abstract void OnWatering();

    public abstract void OnFarmUpdate(float deltaTime);

    public abstract void ResetToInitialState();


    // 이하 함수 빌딩 블록

    protected static TState CommonCropBehavior<TState>(
        List<(Func<TState, TState, bool>, Action<Vector2, Vector2>)> effects,
        Action onPlanted,
        Action<int> onSold,
        Func<TState, TState> transitionFunction,
        TState beforeState,
        Vector2 inputWorldPosition,
        Vector2 cropPosition)
        where TState : struct, ICommonCropState
    {
        var nextState = transitionFunction(beforeState);
        if (IsJustPlanted(beforeState, nextState))
        {
            onPlanted();
        }

        if (beforeState.Harvested && !nextState.Harvested)
        {
            onSold(GetHarvestableCount(beforeState.DecayRatio));
            nextState = ResetCropState(beforeState);
        }

        if (beforeState.DecayRatio >= 1.0f)
        {
            nextState = ResetCropState(beforeState);
        }

        PlayEffectAt(effects, beforeState, nextState)(inputWorldPosition, cropPosition);
        return nextState;
    }

    protected static void ApplySprite(Sprite sprite, SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    protected static int GetHarvestableCount(float decayRatio)
    {
        var freshness = 1.0f - decayRatio;
        return Mathf.CeilToInt(freshness * 10.0f) * 10;
    }

    /// <summary>
    /// 이전 상태가 Planted && !Watered일 경우 다음 상태에 Watered = true로 설정하여 반환하는 함수.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="beforeState"></param>
    /// <returns></returns>
    protected static TState WaterForNeedOnce<TState>(TState beforeState) where TState : struct, ICommonCropState
    {
        var nextState = beforeState;
        if (beforeState.Planted && !beforeState.Watered)
        {
            nextState.Watered = true;
            nextState.WaterWaitingSeconds = 0.0f;
        }

        return nextState;
    }

    protected static TState Decay<TState>(TState beforeState, float deltaTime) where TState : struct, ICommonCropState
    {
        var nextState = beforeState;
        nextState.DecayRatio =
            beforeState.DecayRatio + deltaTime / (beforeState.DecayRatio > 0.5f ? 10.0f : 20.0f) * 0.5f;
        nextState.DecayRatio = Mathf.Clamp01(nextState.DecayRatio);
        return nextState;
    }

    /// <summary>
    /// 다음 상태의 Watered를 true로 변경하는 메소드. 물을 줄 수 있는지 검증을 한 후에 호출해야 함.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="beforeState"></param>
    /// <returns></returns>
    protected static TState Water<TState>(TState beforeState) where TState : struct, ICommonCropState
    {
        var nextState = beforeState;
        nextState.Watered = true;
        nextState.WaterWaitingSeconds = 0.0f;
        return nextState;
    }

    protected static TState Plant<TState>(TState beforeState) where TState : struct, ICommonCropState
    {
        beforeState.Planted = true;
        return beforeState;
    }

    protected static TState WaitWater<TState>(TState beforeState, float deltaTime)
        where TState : struct, ICommonCropState
    {
        beforeState.WaterWaitingSeconds += deltaTime;
        return beforeState;
    }

    protected static TState Grow<TState>(TState beforeState, float deltaTime) where TState : struct, ICommonCropState
    {
        beforeState.GrowthSeconds += deltaTime;
        return beforeState;
    }

    /// <summary>
    /// 모든 값을 초기값으로 설정한 상태를 반환,
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    protected static TState ResetCropState<TState>(TState _) where TState : struct, ICommonCropState =>
        new TState();

    protected static TState Harvest<TState>(TState beforeState) where TState : struct, ICommonCropState
    {
        beforeState.Harvested = true;
        return beforeState;
    }

    protected static TState DoNothing<TState>(TState beforeState) where TState : struct, ICommonCropState =>
        beforeState;

    protected static bool IsJustPlanted<TState>(TState beforeState, TState afterState)
        where TState : struct, ICommonCropState => !beforeState.Planted && afterState.Planted;

    protected static Action<Vector2, Vector2> PlayEffectAt<TState>(
        List<(Func<TState, TState, bool>, Action<Vector2, Vector2>)> effects, TState beforeState, TState afterState)
    {
        var targetEffects = effects.Where(effect => effect.Item1(beforeState, afterState)).ToList();
        return (inputWorldPosition, cropPosition) =>
        {
            foreach (var (_, effect) in targetEffects)
            {
                effect(inputWorldPosition, cropPosition);
            }
        };
    }

    protected static TState DoNothing_OnHold<TState>(TState beforeState, Vector2 initialWorldPosition,
        Vector2 deltaPosition, bool isEnd, float deltaHoldTime) where TState : struct, ICommonCropState =>
        DoNothing(beforeState);

    protected static TState DoNothing_OnFarmUpdate<TState>(TState beforeState, float deltaTime)
        where TState : struct, ICommonCropState => DoNothing(beforeState);

    // 이펙트 조건 및 실행 함수

    protected static bool WaterEffectCondition<TState>(TState beforeState, TState afterState)
        where TState : struct, ICommonCropState => !beforeState.Watered && afterState.Watered;

    protected static void WaterEffect(Vector2 inputWorldPosition, Vector2 cropPosition) =>
        SoundManager.Instance.PlaySfx("SFX_T_water_oneshot", SoundManager.Instance.waterVolume);

    protected static bool PlantEffectCondition<TState>(TState beforeState, TState afterState)
        where TState : struct, ICommonCropState => !beforeState.Planted && afterState.Planted;

    protected static void PlantEffect(Vector2 inputWorldPosition, Vector2 cropPosition)
    {
        SoundManager.Instance.PlaySfx("SFX_T_plant_seed", SoundManager.Instance.plantVolume);
        EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);
    }

    protected static bool HarvestEffectCondition<TState>(TState beforeState, TState afterState)
        where TState : struct, ICommonCropState => !beforeState.Harvested && afterState.Harvested;

    protected static void HarvestEffect_SoilParticle(Vector2 inputWorldPosition, Vector2 cropPosition)
    {
        EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);
        EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilParticle", cropPosition);
        SoundManager.Instance.PlaySfx("SFX_T_harvest", SoundManager.Instance.harvestVolume);
    }

    protected static void HarvestEffect_SoilDust(Vector2 inputWorldPosition, Vector2 cropPosition)
    {
        EffectPlayer.SceneGlobalInstance.PlayTapEffect(inputWorldPosition);
        EffectPlayer.SceneGlobalInstance.PlayVfx("VFX_T_SoilDust", cropPosition);
        SoundManager.Instance.PlaySfx("SFX_T_harvest", SoundManager.Instance.harvestVolume);
    }
}
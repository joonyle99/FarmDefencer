using UnityEngine;
using CropInterfaces;
using JetBrains.Annotations;
using System;

/// <summary>
/// 씨앗인 경우 그냥 심어지고, 다 자라면 그냥 수확되고, 성장할 때는 물만 필요로 하는 흔한 작물입니다.
/// </summary>
public class NormalCrop : Crop, IWaterable
{
    [Serializable]
    public struct CropStage
    {
        public Sprite CropSprite;
        public float GrowthSecondsRequired;
        public float WaterRequired;

        public float WaterConsumptionPerSecond => WaterRequired / GrowthSecondsRequired;
    }
    [Header("설정값 - 작물 성장 단계들")]
    [Tooltip("성장 단계에 따라 순서대로 정의되어야 하며, 0단계는 씨앗 단계, 마지막 단계는 수확 단계여야 합니다. 빈 칸이 없어야 합니다.")]
    public CropStage[] CropStages;
    [Header("설정값 - 작물 물 부족 상태 단계")]
    public CropStage CropLackWaterStage;
    [Header("설정값 - 물이 없을 때 정상 상태에서 시들한 상태로 전환될때까지의 시간(초)")]    public float WaterLackDangerSeconds;
    [Header("설정값 - 물이 없을 때 시들한 상태에서 씨앗 상태로 돌아갈때까지의 시간(초)")]
    public float WaterLackDeadSeconds;
    [Space]
    [Header("상태값 - 현재 성장 단계 인덱스")]
    [Tooltip("CropStages 배열의 인덱스입니다.")]
    public int CurrentStageIndex;
    [Header("상태값 - 현재 단계 진행도(초)")]
    public float CurrentStageAgeSeconds;
    [Header("상태값 - 물 저장량(L)")]
    [Tooltip("땅이 아니라 식물이 저장합니다. 새로 심거나 성장한 직후에 0이 됩니다.")]
    public float WaterStored;
    [Header("상태값 - 물 부족 상태 진입까지 남은 시간(초)")]
    [Tooltip("물을 주면")]
    public float WaterWaitingSeconds;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// 씨앗 상태인 경우 심습니다.
    /// </summary>
    /// <returns></returns>
    public override bool TryPlant()
    {
        if (CurrentStageIndex != 0)
        {
            return false;
        }

        CurrentStageAgeSeconds = 0.0f;
        WaterStored = 0.0f;
        CurrentStageIndex += 1;
        WaterWaitingSeconds = 0.0f;
        return true;
    }

    /// <summary>
    /// 마지막 단계인 경우 수확합니다.
    /// </summary>
    /// <returns></returns>
    public override bool TryHarvest()
    {
        if (CurrentStageIndex == CropStages.Length - 1)
        {
            CurrentStageIndex = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 현재 단계를 모두 지낸 작물이면 성장시킵니다.
    /// </summary>
    /// <returns></returns>
    public override bool TryGrowUp()
    {
        if (CurrentStageIndex == CropStages.Length - 1
           || CurrentStageIndex != 0 && CurrentStageAgeSeconds < CropStages[CurrentStageIndex].GrowthSecondsRequired)
        {
            return false;
        }

        WaterWaitingSeconds = 0.0f;
        WaterStored = 0.0f;
        CurrentStageAgeSeconds = 0.0f;
        CurrentStageIndex++;
        return true;
    }

    /// <summary>
    /// 작물에 물을 줍니다.
    /// 씨앗 단계 또는 수확 단계가 아니어야 합니다.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>씨앗 단계 또는 수확 단계가 아니어서 작물에 물을 성공적으로 준 경우 true, 이외의 경우 false</returns>
    public bool TryWatering(float amount)
    {
        if (CurrentStageIndex == 0 || CurrentStageIndex == CropStages.Length - 1)
        {
            return false;
        }

        WaterStored += amount;
        WaterWaitingSeconds = 0.0f;
        return true;
    }

    /// <summary>
    /// 입력된 상태값에 따라 적절한 스프라이트를 계산하는 순수 메소드입니다.
    /// </summary>
    /// <returns></returns>
    [Pure]
    private Sprite GetSpriteOfCurrentStatus()
    {
        if (CurrentStageIndex == 0)
        {
            return CropStages[0].CropSprite;
        }
        else if (CurrentStageIndex == CropStages.Length - 1)
        {
            return CropStages[CropStages.Length - 1].CropSprite;
        }
        else
        {
            if (WaterStored <= 0.0f && WaterWaitingSeconds >= WaterLackDangerSeconds)
            {
                return CropLackWaterStage.CropSprite;
            }
            else
            {
                return CropStages[CurrentStageIndex].CropSprite;
            }
        }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = CropStages[CurrentStageIndex].CropSprite;
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var spriteOfCurrentStatus = GetSpriteOfCurrentStatus();

        if (_spriteRenderer.sprite != spriteOfCurrentStatus)
        {
            _spriteRenderer.sprite = spriteOfCurrentStatus;
        }

        if (CurrentStageIndex > 0 && CurrentStageIndex < CropStages.Length - 1)
        {
            var currentStage = CropStages[CurrentStageIndex];
            if (CurrentStageAgeSeconds < currentStage.GrowthSecondsRequired) // 아직 성장이 더 필요한 상태
            {
                if (WaterStored >= CropStages[CurrentStageIndex].WaterConsumptionPerSecond * deltaTime) // 물이 남아있는 상태
                {
                    CurrentStageAgeSeconds += deltaTime;
                    WaterStored -= CropStages[CurrentStageIndex].WaterConsumptionPerSecond * deltaTime;
                }
                else // 물이 없는 상태
                {
                    WaterStored = 0.0f;
                    if (WaterWaitingSeconds >= WaterLackDangerSeconds + WaterLackDeadSeconds)
                    {
                        CurrentStageIndex = 0;
                    }
                    WaterWaitingSeconds += deltaTime;
                }
            }
        }
    }
}

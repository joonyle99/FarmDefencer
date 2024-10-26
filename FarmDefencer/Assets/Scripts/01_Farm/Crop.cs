using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 작물을 의미하는 클래스.<br/>
/// 구체적으로는 Field의 한 칸을 의미합니다. 즉 아직 심어지지 않은 한 칸도 의미합니다.
/// <br/><br/>
/// 설정값으로 표시된 필드들은 게임 플레이 흐름에 의해 자동으로 변하지 않는 반드시 사전에 설정해야 하는 값들입니다.<br/>
/// 상태값으로 표시된 필드들은 게임 플레이 흐름에 따라 변하는 값들로, 필요할 경우 임의로 설정할 수 있으며 별도로 조작하지 않아도 됩니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
    [Header("설정값 - 작물 성장 단계들")]
    [Tooltip("성장 단계에 따라 순서대로 정의되어야 하며, 0단계는 씨앗 단계, 마지막 단계는 수확 단계여야 합니다. 빈 칸이 없어야 합니다.")]
    public CropStage[] CropStages;
    [Header("설정값 - 작물 물 부족 상태 단계")]
    public CropStage CropLackWaterStage;
	[Header("설정값 - 물이 없을 때 정상 상태에서 시들한 상태로 전환될때까지의 시간(초)")]
	public float WaterLackDangerSeconds;
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
    /// 이 Crop이 해당 좌표에 해당하는지 확인합니다.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// 씨앗 상태일 경우 작물을 심고 성장을 시작합니다.<br/>
    /// </summary>
    /// <returns>씨앗 상태이고 작물을 심은 경우 true, 씨앗 상태가 아닌 경우 false</returns>
    public bool TryPlant()
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
    /// 작물을 수확합니다.
    /// </summary>
    /// <returns>작물이 마지막 단계인 경우 true, 이외의 경우 false</returns>
    public bool TryHarvest()
    {
        if (CurrentStageIndex == CropStages.Length-1)
        {
            CurrentStageIndex = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 작물을 성장시킵니다.
    /// </summary>
    /// <returns>성장 조건에 부합해 작물을 성장시킨 경우 true, 이외의 경우 false</returns>
    public bool TryGrowUp()
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
        if (CurrentStageIndex == 0 || CurrentStageIndex == CropStages.Length-1)
        {
            return false;
        }

        WaterStored += amount;
        WaterWaitingSeconds = 0.0f;
        return true;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = CropStages[CurrentStageIndex].CropSprite;
    }

	private void Update()
    {
        var deltaTime = Time.deltaTime;

        var spriteOfCurrentStatus = GetSpriteOfCurrentStatus(
            CropStages,
            CropLackWaterStage,
            CurrentStageIndex,
            WaterWaitingSeconds,
            WaterLackDangerSeconds,
            WaterStored);
        if (_spriteRenderer.sprite != spriteOfCurrentStatus)
        {
            _spriteRenderer.sprite = spriteOfCurrentStatus;
        }

        if (CurrentStageIndex > 0 && CurrentStageIndex < CropStages.Length-1)
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

    /// <summary>
    /// 입력된 상태값에 따라 적절한 스프라이트를 계산하는 순수 메소드입니다.
    /// </summary>
    /// <param name="cropStages"></param>
    /// <param name="cropLackWaterStage"></param>
    /// <param name="currentStageIndex"></param>
    /// <param name="waterWaitingSeconds"></param>
    /// <param name="waterLackDangerSeconds"></param>
    /// <param name="waterStored"></param>
    /// <returns></returns>
    private static Sprite GetSpriteOfCurrentStatus(
        CropStage[] cropStages, 
        CropStage cropLackWaterStage,
        int currentStageIndex, 
        float waterWaitingSeconds,
        float waterLackDangerSeconds,
        float waterStored)
    {
        if (currentStageIndex == 0)
        {
            return cropStages[0].CropSprite;
        }
        else if (currentStageIndex == cropStages.Length - 1)
        {
            return cropStages[cropStages.Length - 1].CropSprite;
        }
        else
        {
            if (waterStored <= 0.0f && waterWaitingSeconds >= waterLackDangerSeconds)
            {
                return cropLackWaterStage.CropSprite;
            }
            else
            {
                return cropStages[currentStageIndex].CropSprite;
            }
        }
    }
}
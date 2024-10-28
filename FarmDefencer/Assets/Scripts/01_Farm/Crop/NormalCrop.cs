using UnityEngine;
using CropInterfaces;
using JetBrains.Annotations;
using System;

/// <summary>
/// ������ ��� �׳� �ɾ�����, �� �ڶ�� �׳� ��Ȯ�ǰ�, ������ ���� ���� �ʿ�� �ϴ� ���� �۹��Դϴ�.
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
    [Header("������ - �۹� ���� �ܰ��")]
    [Tooltip("���� �ܰ迡 ���� ������� ���ǵǾ�� �ϸ�, 0�ܰ�� ���� �ܰ�, ������ �ܰ�� ��Ȯ �ܰ迩�� �մϴ�. �� ĭ�� ����� �մϴ�.")]
    public CropStage[] CropStages;
    [Header("������ - �۹� �� ���� ���� �ܰ�")]
    public CropStage CropLackWaterStage;
    [Header("������ - ���� ���� �� ���� ���¿��� �õ��� ���·� ��ȯ�ɶ������� �ð�(��)")]    public float WaterLackDangerSeconds;
    [Header("������ - ���� ���� �� �õ��� ���¿��� ���� ���·� ���ư��������� �ð�(��)")]
    public float WaterLackDeadSeconds;
    [Space]
    [Header("���°� - ���� ���� �ܰ� �ε���")]
    [Tooltip("CropStages �迭�� �ε����Դϴ�.")]
    public int CurrentStageIndex;
    [Header("���°� - ���� �ܰ� ���൵(��)")]
    public float CurrentStageAgeSeconds;
    [Header("���°� - �� ���差(L)")]
    [Tooltip("���� �ƴ϶� �Ĺ��� �����մϴ�. ���� �ɰų� ������ ���Ŀ� 0�� �˴ϴ�.")]
    public float WaterStored;
    [Header("���°� - �� ���� ���� ���Ա��� ���� �ð�(��)")]
    [Tooltip("���� �ָ�")]
    public float WaterWaitingSeconds;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// ���� ������ ��� �ɽ��ϴ�.
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
    /// ������ �ܰ��� ��� ��Ȯ�մϴ�.
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
    /// ���� �ܰ踦 ��� ���� �۹��̸� �����ŵ�ϴ�.
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
    /// �۹��� ���� �ݴϴ�.
    /// ���� �ܰ� �Ǵ� ��Ȯ �ܰ谡 �ƴϾ�� �մϴ�.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>���� �ܰ� �Ǵ� ��Ȯ �ܰ谡 �ƴϾ �۹��� ���� ���������� �� ��� true, �̿��� ��� false</returns>
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
    /// �Էµ� ���°��� ���� ������ ��������Ʈ�� ����ϴ� ���� �޼ҵ��Դϴ�.
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
            if (CurrentStageAgeSeconds < currentStage.GrowthSecondsRequired) // ���� ������ �� �ʿ��� ����
            {
                if (WaterStored >= CropStages[CurrentStageIndex].WaterConsumptionPerSecond * deltaTime) // ���� �����ִ� ����
                {
                    CurrentStageAgeSeconds += deltaTime;
                    WaterStored -= CropStages[CurrentStageIndex].WaterConsumptionPerSecond * deltaTime;
                }
                else // ���� ���� ����
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

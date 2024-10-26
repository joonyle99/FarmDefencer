using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �۹��� �ǹ��ϴ� Ŭ����.<br/>
/// ��ü�����δ� Field�� �� ĭ�� �ǹ��մϴ�. �� ���� �ɾ����� ���� �� ĭ�� �ǹ��մϴ�.
/// <br/><br/>
/// ���������� ǥ�õ� �ʵ���� ���� �÷��� �帧�� ���� �ڵ����� ������ �ʴ� �ݵ�� ������ �����ؾ� �ϴ� �����Դϴ�.<br/>
/// ���°����� ǥ�õ� �ʵ���� ���� �÷��� �帧�� ���� ���ϴ� �����, �ʿ��� ��� ���Ƿ� ������ �� ������ ������ �������� �ʾƵ� �˴ϴ�.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
    [Header("������ - �۹� ���� �ܰ��")]
    [Tooltip("���� �ܰ迡 ���� ������� ���ǵǾ�� �ϸ�, 0�ܰ�� ���� �ܰ�, ������ �ܰ�� ��Ȯ �ܰ迩�� �մϴ�. �� ĭ�� ����� �մϴ�.")]
    public CropStage[] CropStages;
    [Header("������ - �۹� �� ���� ���� �ܰ�")]
    public CropStage CropLackWaterStage;
	[Header("������ - ���� ���� �� ���� ���¿��� �õ��� ���·� ��ȯ�ɶ������� �ð�(��)")]
	public float WaterLackDangerSeconds;
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
    /// �� Crop�� �ش� ��ǥ�� �ش��ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// ���� ������ ��� �۹��� �ɰ� ������ �����մϴ�.<br/>
    /// </summary>
    /// <returns>���� �����̰� �۹��� ���� ��� true, ���� ���°� �ƴ� ��� false</returns>
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
    /// �۹��� ��Ȯ�մϴ�.
    /// </summary>
    /// <returns>�۹��� ������ �ܰ��� ��� true, �̿��� ��� false</returns>
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
    /// �۹��� �����ŵ�ϴ�.
    /// </summary>
    /// <returns>���� ���ǿ� ������ �۹��� �����Ų ��� true, �̿��� ��� false</returns>
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
    /// �۹��� ���� �ݴϴ�.
    /// ���� �ܰ� �Ǵ� ��Ȯ �ܰ谡 �ƴϾ�� �մϴ�.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>���� �ܰ� �Ǵ� ��Ȯ �ܰ谡 �ƴϾ �۹��� ���� ���������� �� ��� true, �̿��� ��� false</returns>
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

    /// <summary>
    /// �Էµ� ���°��� ���� ������ ��������Ʈ�� ����ϴ� ���� �޼ҵ��Դϴ�.
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
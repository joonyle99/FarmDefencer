/// <summary>
/// ��Ȯ ������ ������ ��� ������ Carrot�� �����ϹǷ� �ش� �ڵ带 �����մϴ�.
/// </summary>
public class CropPotato : CropCarrot
{
	public float HarvestHoldTime = 2.0f;
	public override void OnTap()
	{
		// base.OnTap()�� ������ �޶� ȣ������ ����
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthPercentage >= 100.0f)
			{
				State = CropState.Harvested;
			}
			else
			{
				if (WaterWaitingSeconds <= PlantToDeadSeconds)
				{
					WaterStored += MatureAgeSeconds * 1.1f; // �� �¾ƶ������� �ϸ� 99%���� �� �ٽ� �޶�� �� �� ����
				}
			}
		}
	}

	public override void OnHoldAndUp(float holdTime)
	{
		if (State == CropState.Harvested && holdTime >= HarvestHoldTime)
		{
			OnHarvest();
			State = CropState.Seed;
		}
	}
}
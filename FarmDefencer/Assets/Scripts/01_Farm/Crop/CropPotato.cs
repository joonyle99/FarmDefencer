/// <summary>
/// 수확 동작을 제외한 모든 동작이 Carrot과 동일하므로 해당 코드를 재사용합니다.
/// </summary>
public class CropPotato : CropCarrot
{
	public float HarvestHoldTime = 2.0f;
	public override void OnTap()
	{
		// base.OnTap()는 동작이 달라 호출하지 않음
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
					WaterStored += MatureAgeSeconds * 1.1f; // 딱 맞아떨어지게 하면 99%에서 물 다시 달라고 할 수 있음
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
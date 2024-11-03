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
		else if (State == CropState.Harvested)
		{
			OnHarvest();
			State = CropState.Seed;
		}
	}

	public override void OnHolding(float holdTime)
	{
		if (State == CropState.Planted
			&& GrowthPercentage >= 100.0f
			&& holdTime >= HarvestHoldTime)
		{
			State = CropState.Harvested;
		}
	}
}
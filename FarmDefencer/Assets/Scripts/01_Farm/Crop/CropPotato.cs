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
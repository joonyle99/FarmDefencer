using UnityEngine;

/// <summary>
/// ��Ȯ ������ ������ ��� ������ Carrot�� �����ϹǷ� �ش� �ڵ带 �����մϴ�.
/// </summary>
public class CropPotato : CropCarrot
{
	public float HarvestHoldTime = 2.0f;
	private float _holdTimeElapsed;
	public override void OnSingleTap()
	{
		// base.OnTap()�� ������ �޶� ȣ������ ����
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Harvested)
		{
			Itemify();
		}
	}

	public override void OnSingleHolding(Vector2 _, bool isEnd, float deltaHoldTime)
	{
		if (isEnd)
		{
			_holdTimeElapsed = 0.0f;
			return;
		}

		if (State == CropState.Planted && GrowthPercentage >= 100.0f)
		{
			_holdTimeElapsed += deltaHoldTime;
			if (_holdTimeElapsed >= HarvestHoldTime)
			{
				State = CropState.Harvested;
			}
		}
	}
}
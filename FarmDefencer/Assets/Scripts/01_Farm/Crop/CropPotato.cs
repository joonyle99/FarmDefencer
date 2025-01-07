using UnityEngine;

/// <summary>
/// 수확 동작을 제외한 모든 동작이 Carrot과 동일하므로 해당 코드를 재사용합니다.
/// </summary>
public class CropPotato : CropCarrot
{
	public float HarvestHoldTime = 2.0f;
	private float _holdTimeElapsed;
	public override void OnSingleTap()
	{
		// base.OnTap()는 동작이 달라 호출하지 않음
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
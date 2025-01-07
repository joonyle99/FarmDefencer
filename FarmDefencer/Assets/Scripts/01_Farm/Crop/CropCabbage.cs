using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropCabbage : CropCorn
{
	public float DeltaShakeCriterion = 100.0f;
	public int ShakeCountCriterion = 4;
	private int _shakeCount;
	private bool _wasLastShakeLeft;
	public override void OnSingleTap()
	{
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Harvested)
		{
			Itemify();
		}
	}

	public override void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		if (isEnd)
		{
			_shakeCount = 0;
			return;
		}

		if (State == CropState.Planted
			&& GrowthPercentage >= 100.0f)
		{
			var deltaShake = deltaPosition.x;
			if (Mathf.Abs(deltaShake) > DeltaShakeCriterion)
			{
				if (_shakeCount == 0)
				{
					_shakeCount += 1;
					_wasLastShakeLeft = deltaShake < 0.0f;
				}
				else
				{
					if (deltaShake < 0.0f && !_wasLastShakeLeft
						|| deltaShake > 0.0f && _wasLastShakeLeft)
					{
						_shakeCount += 1;
						_wasLastShakeLeft = !_wasLastShakeLeft;
					}
				}
				Debug.Log($"Shaked { (_wasLastShakeLeft ? "Left" : "Right") }, Count: {_shakeCount}");

				if (_shakeCount >= ShakeCountCriterion)
				{
					State = CropState.Harvested;
				}
			}
		}
	}
}

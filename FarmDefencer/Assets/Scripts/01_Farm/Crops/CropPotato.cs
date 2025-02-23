using UnityEngine;

public class CropPotato : Crop
{
	private const float MatureSeconds = 15.0f;
	private const float HarvestHoldTime = 2.0f;

	public Sprite SeedSprite;
	public Sprite MatureSprite;
	public Sprite BeforeWaterSprite;
	public Sprite DeadSprite;
	public Sprite GrowingSprite;
	public Sprite HarvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private bool _isSeed;
	private bool _watered;
	private bool _harvested;
	private float _holdingTime;

	public override void OnSingleTap()
	{
		if (_isSeed)
		{
			SoundManager.PlaySfx("SFX_plant_seed");
			_isSeed = false;
		}
		else if (_harvested)
		{
			if (HarvestHandler(1) > 0)
			{
				_isSeed = true;
			}
		}
	}

	public override void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		if (growthSeconds >= MatureSeconds && !_harvested)
		{
			_holdingTime += deltaHoldTime;
			if (_holdingTime >= HarvestHoldTime)
			{
				_holdingTime = 0.0f;
				_harvested = true;
				SoundManager.PlaySfx("SFX_harvest");
			}
		}

		if (isEnd)
		{
			_holdingTime = 0.0f;
		}
	}

	public override void OnWatering()
	{
		if (!_isSeed && !_watered)
		{
			waterWaitingSeconds = 0.0f;
			_watered = true;
			SoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		if (_isSeed)
		{
			_watered = false;
			_harvested = false;
			waterWaitingSeconds = 0.0f;
			growthSeconds = 0.0f;
			_holdingTime = 0.0f;

			if (_spriteRenderer.sprite != SeedSprite)
			{
				_spriteRenderer.sprite = SeedSprite;
			}

			return;
		}

		if (_harvested)
		{
			if (_spriteRenderer.sprite != HarvestedSprite)
			{
				_spriteRenderer.sprite = HarvestedSprite;
			}

			return;
		}

		if (growthSeconds >= MatureSeconds)
		{
			if (_spriteRenderer.sprite != MatureSprite)
			{
				_spriteRenderer.sprite = MatureSprite;
			}

			return;
		}

		if (!_watered)
		{
			waterWaitingSeconds += deltaTime;

			if (waterWaitingSeconds >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds)
			{
				_isSeed = true;
			}
			else if (waterWaitingSeconds >= WaterWaitingDeadSeconds)
			{
				if (_spriteRenderer.sprite != DeadSprite)
				{
					_spriteRenderer.sprite = DeadSprite;
				}
			}
			else
			{
				if (_spriteRenderer.sprite != BeforeWaterSprite)
				{
					_spriteRenderer.sprite = BeforeWaterSprite;
				}
			}

			return;
		}

		growthSeconds += deltaTime;
		if (growthSeconds >= MatureSeconds)
		{
			if (_spriteRenderer.sprite != MatureSprite)
			{
				_spriteRenderer.sprite = MatureSprite;
			}
			return;
		}

		if (_spriteRenderer.sprite != GrowingSprite)
		{
			_spriteRenderer.sprite = GrowingSprite;
		}
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_isSeed = true;
		_watered = false;
		_harvested = false;
		_holdingTime = 0.0f;
	}
}

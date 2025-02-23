using UnityEngine;

public class CropCarrot : Crop
{
	private const float MatureSeconds = 15.0f;

	public Sprite SeedSprite;
	public Sprite MatureSprite;
	public Sprite BeforeWaterSprite;
	public Sprite DeadSprite;
	public Sprite GrowingSprite;
	public Sprite HarvestedSprite;


	private SpriteRenderer _spriteRenderer;
	private bool _isSeed;
	private bool _harvested;

	public override void OnSingleTap()
	{
		if (_isSeed)
		{
			SoundManager.PlaySfx("SFX_plant_seed");
			_isSeed = false;
		}
		else if (growthSeconds >= MatureSeconds && !_harvested)
		{
			SoundManager.PlaySfx("SFX_harvest");
			_harvested = true;
		}
		else if (_harvested)
		{
			if (HarvestHandler(1) > 0)
			{
				_isSeed = true;
			}
		}
	}

	public override void OnWatering()
	{
		if (!_isSeed && !watered)
		{
			waterWaitingSeconds = 0.0f;
			watered = true;
			SoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		if (_isSeed)
		{
			watered = false;
			_harvested = false;
			waterWaitingSeconds = 0.0f;
			growthSeconds = 0.0f;

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

		if (!watered)
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
		watered = false;
		_harvested = false;
	}
}

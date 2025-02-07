using UnityEngine;

public class CropCorn : Crop
{
	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	public float Stage1GrowthSeconds = 15.0f;
	[Space]
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	public float Stage2GrowthSeconds = 15.0f;
	[Space]
	public Sprite MatureSprite;
	public Sprite HarvestedSprite;

	private bool _isSeed;
	private bool _harvested;
	private SpriteRenderer _spriteRenderer;

	public override void OnSingleTap()
	{
		if (_isSeed)
		{
			FarmSoundManager.PlaySfx("SFX_plant_seed");
			_isSeed = false;
		}
		else if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds && !_harvested)
		{
			FarmSoundManager.PlaySfx("SFX_harvest");
			_harvested = true;
		}
		else if (_harvested)
		{
			if (HarvestHandler(1)>0)
			{
				_isSeed = true;
			}
		}
	}

	public override void OnWatering()
	{
		if (!_isSeed && !watered)
		{
			watered = true;
			FarmSoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		if (_isSeed)
		{
			growthSeconds = 0.0f;
			waterWaitingSeconds = 0.0f;
			watered = false;
			_harvested = false;
			_spriteRenderer.sprite = SeedSprite;
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

		if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 모두 성장한 단계
		{
			if (_spriteRenderer.sprite != MatureSprite)
			{
				_spriteRenderer.sprite = MatureSprite;
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds) // 성장 2단계
		{
			if (!watered) // 물 안준 상태
			{
				waterWaitingSeconds += deltaTime;
				if (waterWaitingSeconds >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds)
				{
					_isSeed = true;
				}
				else if (waterWaitingSeconds >= WaterWaitingDeadSeconds)
				{
					if (_spriteRenderer.sprite != Stage2DeadSprite)
					{
						_spriteRenderer.sprite = Stage2DeadSprite;
					}
				}
				else
				{
					if (_spriteRenderer.sprite != Stage2BeforeWateringSprite)
					{
						_spriteRenderer.sprite = Stage2BeforeWateringSprite;
					}
				}
			}
			else // 물 준 상태
			{
				growthSeconds += deltaTime;
				if (_spriteRenderer.sprite != Stage2AfterWateringSprite)
				{
					_spriteRenderer.sprite = Stage2AfterWateringSprite;
				}
			}
		}
		else // 성장 1단계
		{
			if (!watered) // 물 안준 상태
			{
				waterWaitingSeconds += deltaTime;
				if (waterWaitingSeconds >= WaterWaitingDeadSeconds + WaterWaitingResetSeconds)
				{
					_isSeed = true;
				}
				else if (waterWaitingSeconds >= WaterWaitingDeadSeconds)
				{
					if (_spriteRenderer.sprite != Stage1DeadSprite)
					{
						_spriteRenderer.sprite = Stage1DeadSprite;
					}
				}
				else
				{
					if (_spriteRenderer.sprite != Stage1BeforeWateringSprite)
					{
						_spriteRenderer.sprite = Stage1BeforeWateringSprite;
					}
				}
			}
			else // 물 준 상태
			{
				growthSeconds += deltaTime;
				
				if (growthSeconds >= Stage1GrowthSeconds)
				{
					watered = false;
				}

				if (_spriteRenderer.sprite != Stage1AfterWateringSprite)
				{
					_spriteRenderer.sprite = Stage1AfterWateringSprite;
				}
			}
		}
	}

	private void Awake()
	{
		_isSeed = true;
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}
}

using UnityEngine;

public class CropCucumber : Crop
{
	private const float Stage1GrowthSeconds = 15.0f;
	private const float Stage2GrowthSeconds = 15.0f;

	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	[Space]
	public Sprite BeforeShortTrellisSprite;
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	[Space]
	public Sprite BeforeLongTrellisSprite;
	public Sprite MatureSprite;
	public Sprite HarvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private bool _shortTrellisPlaced;
	private bool _longTrellisPlaced;
	private bool _isSeed;
	private bool _harvested;

	public override void OnWatering()
	{
		if (!watered && !_isSeed
			&& (growthSeconds == 0.0f || growthSeconds >= Stage1GrowthSeconds && _shortTrellisPlaced))
		{
			waterWaitingSeconds = 0.0f;
			watered = true;
			SoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnSingleTap()
	{
		if (_isSeed)
		{
			_isSeed = false;
			SoundManager.PlaySfx("SFX_plant_seed");
		}
		else if (!_shortTrellisPlaced && !_longTrellisPlaced
				&& growthSeconds >= Stage1GrowthSeconds)
		{
			_shortTrellisPlaced = true;
		}
		else if (_shortTrellisPlaced && !_longTrellisPlaced
			&& growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds)
		{
			_longTrellisPlaced = true;
		}
		else if (!_harvested 
			&&_shortTrellisPlaced && _longTrellisPlaced
			&& growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds)
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

	public override void OnFarmUpdate(float deltaTime)
	{
		if (_isSeed)
		{
			growthSeconds = 0.0f;
			waterWaitingSeconds = 0.0f;
			watered = false;
			_harvested = false;
			_shortTrellisPlaced = false;
			_longTrellisPlaced = false;

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

		if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 모두 성장한 단계
		{
			if (!_longTrellisPlaced) // 긴 지지대 설치 이전 상태
			{
				if (_spriteRenderer.sprite != BeforeLongTrellisSprite)
				{
					_spriteRenderer.sprite = BeforeLongTrellisSprite;
				}
			}
			else
			{
				if (_spriteRenderer.sprite != MatureSprite)
				{
					_spriteRenderer.sprite = MatureSprite;
				}
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds) // 성장 2단계
		{
			if (!_shortTrellisPlaced) // 짧은 지지대 설치 이전 상태
			{
				if (_spriteRenderer.sprite != BeforeShortTrellisSprite)
				{
					_spriteRenderer.sprite = BeforeShortTrellisSprite;
				}
			}
			else if (!watered) // 물 안준 상태
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


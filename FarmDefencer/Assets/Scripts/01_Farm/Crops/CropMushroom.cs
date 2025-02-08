using UnityEngine;

public class CropMushroom : Crop
{
	public float PlantRubbingCriterion = 10.0f;
	[Space]
	public Sprite Stage1BeforeWaterSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWaterSprite;
	public float Stage1GrowthSeconds = 20.0f;
	[Space]
	public Sprite Stage2BeforeWaterSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWaterSprite;
	public float Stage2GrowthSeconds = 20.0f;
	[Space]
	public Sprite Stage3BeforeInoculationSprite;
	public Sprite Stage3AfterInoculationSprite;
	public float Stage3GrowthSeconds = 10.0f;
	private bool _inoculated;
	[Space]
	public Sprite Stage4NormalSprite;
	public Sprite Stage4PoisonousSprite;
	[Space]
	public Sprite HarvestedNormalSprite;
	public Sprite HarvestedPoisonousSprite;

	private SpriteRenderer _spriteRenderer;
	private float _holdingTime;
	private bool _poisonousDetermined;
	private bool _poisonous;
	private bool _isSeed;
	private bool _harvested;
	private int _tapCount;
	private float _lastTapTime;
	private bool _boom;
	private float _boomElapsedTime;

	public override void OnSingleTap()
{
		if (!_harvested
			&& growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds)
		{
			_harvested = true;
			FarmSoundManager.PlaySfx("SFX_harvest");
		}
		else if (_harvested)
		{
			var currentTime = Time.time;
			if (_lastTapTime + 0.3f > currentTime)
			{
				_tapCount++;
			}
			else
			{
				_tapCount = 1;
			}
			_lastTapTime = currentTime;

			if (_tapCount >= 5)
			{
				if (_poisonous)
				{
					_boom = true;
				}
				else
				{
					if (HarvestHandler(1) > 0)
					{
						_isSeed = true;
					}
				}
			}
		}
	}

	public override void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		if (isEnd)
		{
			_holdingTime = 0.0f;
		}

		if (_isSeed)
		{
			var deltaX = deltaPosition.x;
			if (Mathf.Abs(deltaX) > PlantRubbingCriterion)
			{
				_isSeed = false;
				FarmSoundManager.PlaySfx("SFX_plant_seed");
			}
		}
		else if (!_inoculated
			&&growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds)
		{
			_holdingTime += deltaHoldTime;
			if (_holdingTime >= 2.0f)
			{
				_inoculated = true;
				_holdingTime = 0.0f;
			}
		}
	}

	public override void OnWatering()
	{
		if (!_isSeed && !watered
			&& (growthSeconds == 0.0f || growthSeconds >= Stage1GrowthSeconds && growthSeconds < Stage1GrowthSeconds+Stage2GrowthSeconds))
		{
			watered = true;
			waterWaitingSeconds = 0.0f;
			FarmSoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		if (_isSeed)
		{
			watered = false;
			waterWaitingSeconds = 0.0f;
			growthSeconds = 0.0f;
			_harvested = false;
			_inoculated = false;
			_poisonousDetermined = false;
			_boom = false;
			_boomElapsedTime = 0.0f;
			_tapCount = 1;

			if (_spriteRenderer.sprite != null)
			{
				_spriteRenderer.sprite = null;
			}

			return;
		}

		if (_harvested)
		{
			if (_poisonous)
			{
				if (_boom)
				{
					_boomElapsedTime += deltaTime;
					if (_boomElapsedTime >= 5.0f)
					{
						_isSeed = true;
					}
				}

				if (_spriteRenderer.sprite != HarvestedPoisonousSprite)
				{
					_spriteRenderer.sprite = HarvestedPoisonousSprite;
				}
			}
			else
			{
				if (_spriteRenderer.sprite != HarvestedNormalSprite)
				{
					_spriteRenderer.sprite = HarvestedNormalSprite;
				}
			}

			return;
		}

		if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds) // 모두 성장한 단계
		{
			if (!_poisonousDetermined)
			{
				_poisonousDetermined = true;
				_poisonous = Random.Range(0.0f, 1.0f) <= 0.65f;
				_spriteRenderer.sprite = _poisonous ? Stage4PoisonousSprite : Stage4NormalSprite;
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 성장 3단계
		{
			if (_inoculated)
			{
				growthSeconds += deltaTime;

				if (_spriteRenderer.sprite != Stage3AfterInoculationSprite)
				{
					_spriteRenderer.sprite = Stage3AfterInoculationSprite;
				}
			}
			else
			{
				if (_spriteRenderer.sprite != Stage3BeforeInoculationSprite)
				{
					_spriteRenderer.sprite = Stage3BeforeInoculationSprite;
				}
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
					if (_spriteRenderer.sprite != Stage2BeforeWaterSprite)
					{
						_spriteRenderer.sprite = Stage2BeforeWaterSprite;
					}
				}
			}
			else // 물 준 상태
			{
				growthSeconds += deltaTime;

				if (_spriteRenderer.sprite != Stage2AfterWaterSprite)
				{
					_spriteRenderer.sprite = Stage2AfterWaterSprite;
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
					if (_spriteRenderer.sprite != Stage1BeforeWaterSprite)
					{
						_spriteRenderer.sprite = Stage1BeforeWaterSprite;
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

				if (_spriteRenderer.sprite != Stage1AfterWaterSprite)
				{
					_spriteRenderer.sprite = Stage1AfterWaterSprite;
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

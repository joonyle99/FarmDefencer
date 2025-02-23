using UnityEngine;

public class CropSweetpotato : Crop
{
	private const float PlantRubbingCriterion = 5.0f; // 밭 문지르기 동작 판정 기준 (가로 방향 위치 델타)
	private const float Stage1GrowthSeconds = 15.0f;
	private const float Stage2GrowthSeconds = 15.0f;
	private const float Stage3GrowthSeconds = 10.0f;
	private const float Stage4GrowthSeconds = 5.0f;

	public Sprite Stage1BeforeWaterSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWaterSprite;
	[Space]
	public Sprite Stage2BeforeWaterSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWaterSprite;
	[Space]
	public Sprite Stage3BeforeWrapSprite;
	public Sprite Stage3AfterWrapSprite;
	private bool _wrapped = false;
	[Space]
	public Sprite Stage4Sprite;
	[Space]
	public Sprite Stage5_X_Sprite;
	public Sprite Stage5_O_Sprite;
	[Space]
	public Sprite Stage5_OO_Sprite;
	public Sprite Stage5_OX_Sprite;
	public Sprite Stage5_XO_Sprite;
	public Sprite Stage5_XX_Sprite;
	[Space]
	public Sprite Stage5_XXX_Sprite;
	public Sprite Stage5_XXO_Sprite;
	public Sprite Stage5_XOX_Sprite;
	public Sprite Stage5_OXX_Sprite;
	public Sprite Stage5_XOO_Sprite;
	public Sprite Stage5_OXO_Sprite;
	public Sprite Stage5_OOX_Sprite;
	public Sprite Stage5_OOO_Sprite;
	[Space]
	public Sprite Harvested1Sprite;
	public Sprite Harvested2Sprite;
	public Sprite Harvested3Sprite;
	private int _sweetpotatoCount;

	private SpriteRenderer _spriteRenderer;
	private float _lastSingleTapTime;
	private int _tapCount;
	private float _holdingTime;
	private bool _countDetermined;
	private bool _isSeed;
	private bool _harvested;

	public override void OnSingleTap()
	{
		if (!_harvested &&
			growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds + Stage4GrowthSeconds )
		{
			var currentTime = Time.time;
			if (_lastSingleTapTime + 0.3f > currentTime)
			{
				_tapCount++;
			}
			else
			{
				_tapCount = 1;
			}
			_lastSingleTapTime = currentTime;
			if (_tapCount >= 5)
			{
				_harvested = true;
				FarmSoundManager.PlaySfx("SFX_harvest");
			}
		}
		else if (_harvested)
		{
			var itemizedCount = HarvestHandler(_sweetpotatoCount);
			_sweetpotatoCount -= itemizedCount;
			if (_sweetpotatoCount <= 0)
			{
				_isSeed = true;
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
		else if (!_wrapped
			&& growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds)
		{
			_holdingTime += deltaHoldTime;
			if (_holdingTime >= 2.0f)
			{
				_wrapped = true;
				_holdingTime = 0.0f;
			}
		}
	}

	public override void OnWatering()
	{
		if (!watered && !_isSeed
			&& (growthSeconds == 0.0f || growthSeconds >= Stage1GrowthSeconds && growthSeconds < Stage1GrowthSeconds + Stage2GrowthSeconds))
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
			_wrapped = false;
			_tapCount = 1;
			_countDetermined = false;
			_harvested = false;

			if (_spriteRenderer.sprite != null)
			{
				_spriteRenderer.sprite = null;
			}

			return;
		}

		if (_harvested)
		{
			if (_sweetpotatoCount == 0)
			{
				_isSeed = true;
			}
			else
			{
				if (_sweetpotatoCount == 1)
				{
					if (_spriteRenderer.sprite != Harvested1Sprite)
					{
						_spriteRenderer.sprite = Harvested1Sprite;
					}
				}
				else if (_sweetpotatoCount == 2)
				{
					if (_spriteRenderer.sprite != Harvested2Sprite)
					{
						_spriteRenderer.sprite = Harvested2Sprite;
					}
				}
				else // 3
				{
					if (_spriteRenderer.sprite != Harvested3Sprite)
					{
						_spriteRenderer.sprite = Harvested3Sprite;
					}
				}
			}

			return;
		}

		if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds + Stage4GrowthSeconds) // 모두 성장한 단계
		{
			if (!_countDetermined)
			{
				_countDetermined = true;
				var countDecision = Random.Range(0.0f, 1.0f);

				if (countDecision >= 0.0f && countDecision < 0.6f)
				{
					var rotten = Random.Range(0.0f, 1.0f) < 0.2f;
					if (rotten)
					{
						_sweetpotatoCount = 0;
						_spriteRenderer.sprite = Stage5_X_Sprite;
					}
					else
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_O_Sprite;
					}
				}
				else if (countDecision >= 0.6f && countDecision < 0.9f)
				{
					var firstRotten = Random.Range(0.0f, 1.0f) < 0.3f;
					var secondRotten = Random.Range(0.0f, 1.0f) < 0.3f;
					if (firstRotten && secondRotten)
					{
						_sweetpotatoCount = 0;
						_spriteRenderer.sprite = Stage5_XX_Sprite;
					}
					else if (!firstRotten && secondRotten)
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_OX_Sprite;
					}
					else if (firstRotten && !secondRotten)
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_XO_Sprite;
					}
					else
					{
						_sweetpotatoCount = 2;
						_spriteRenderer.sprite = Stage5_OO_Sprite;
					}
				}
				else
				{
					var firstRotten = Random.Range(0.0f, 1.0f) < 0.4f;
					var secondRotten = Random.Range(0.0f, 1.0f) < 0.4f;
					var thirdRotten = Random.Range(0.0f, 1.0f) < 0.4f;
					if (firstRotten && secondRotten && thirdRotten)
					{
						_sweetpotatoCount = 0;
						_spriteRenderer.sprite = Stage5_XXX_Sprite;
					}
					else if (!firstRotten && secondRotten && thirdRotten)
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_OXX_Sprite;
					}
					else if (firstRotten && !secondRotten && thirdRotten)
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_XOX_Sprite;
					}
					else if (firstRotten && secondRotten && !thirdRotten)
					{
						_sweetpotatoCount = 1;
						_spriteRenderer.sprite = Stage5_XXO_Sprite;
					}
					else if (firstRotten && !secondRotten && !thirdRotten)
					{
						_sweetpotatoCount = 2;
						_spriteRenderer.sprite = Stage5_XOO_Sprite;
					}
					else if (!firstRotten && secondRotten && !thirdRotten)
					{
						_sweetpotatoCount = 2;
						_spriteRenderer.sprite = Stage5_OXO_Sprite;
					}
					else if (!firstRotten && !secondRotten && thirdRotten)
					{
						_sweetpotatoCount = 2;
						_spriteRenderer.sprite = Stage5_OOX_Sprite;
					}
					else
					{
						_sweetpotatoCount = 3;
						_spriteRenderer.sprite = Stage5_OOO_Sprite;
					}
				}
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds) // 성장 4단계
		{
			growthSeconds += deltaTime;

			if (_spriteRenderer.sprite != Stage4Sprite)
			{
				_spriteRenderer.sprite = Stage4Sprite;
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 성장 3단계
		{
			if (_wrapped)
			{
				growthSeconds += deltaTime;

				if (_spriteRenderer.sprite != Stage3AfterWrapSprite)
				{
					_spriteRenderer.sprite = Stage3AfterWrapSprite;
				}
			}
			else
			{
				if (_spriteRenderer.sprite != Stage3BeforeWrapSprite)
				{
					_spriteRenderer.sprite = Stage3BeforeWrapSprite;
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

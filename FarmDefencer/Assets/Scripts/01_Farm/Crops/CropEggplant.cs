using UnityEngine;

public class CropEggplant : Crop
{
	private const float DoubleTapCriterion = 0.3f; // 연속 탭 동작 판정 시간. 이 시간 이내로 다시 탭 해야 연속 탭으로 간주됨
	private const float Stage1GrowthSeconds = 10.0f;
	private const float Stage2GrowthSeconds = 10.0f;
	private const int InitialLeavesCount = 2; // 마지막 수확 단계에서의 최초 잎 개수

	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWaterSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWaterSprite;
	[Space]
	public Sprite BeforeTrellisSprite;
	public Sprite Stage2BeforeWaterSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWaterSprite;
	[Space]
	public Sprite MatureFullLeavesSprite;
	public Sprite MatureHalfLeafSprite;
	public Sprite MatureSprite;
	public Sprite HarvestedSprite;

	private SpriteRenderer _spriteRenderer;
	private bool _trellisPlaced;
	private int _leavesRemaining;
	private float _lastSingleTapTime;
	private bool _isSeed;
	private bool _harvested;

	public override void OnWatering()
	{
		if (!watered && !_isSeed
			&& (growthSeconds == 0.0f || growthSeconds >= Stage1GrowthSeconds && _trellisPlaced))
		{
			watered = true;
			waterWaitingSeconds = 0.0f;
			FarmSoundManager.PlaySfx("SFX_water_oneshot");
		}
	}

	public override void OnSingleTap()
	{
		var currentTime = Time.time;
		if (_lastSingleTapTime + DoubleTapCriterion > currentTime)
		{
			if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds && _leavesRemaining > 0)
			{
				_leavesRemaining -= 1;
				_lastSingleTapTime = currentTime - DoubleTapCriterion; // 연속 입력 판정 방지
				return; // 액션 소모
			}
		}
		_lastSingleTapTime = currentTime;

		if (_isSeed)
		{
			_isSeed = false;
			FarmSoundManager.PlaySfx("SFX_plant_seed");
		}
		else if (growthSeconds >= Stage1GrowthSeconds
			&& !_trellisPlaced)
		{
			_trellisPlaced = true;
		}
		else if (!_harvested && growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds && _leavesRemaining == 0)
		{
			_harvested = true;
			FarmSoundManager.PlaySfx("SFX_harvest");
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
			watered = false;
			waterWaitingSeconds = 0.0f;
			growthSeconds = 0.0f;
			_harvested = false;
			_trellisPlaced = false;
			_spriteRenderer.sprite = SeedSprite;
			_leavesRemaining = InitialLeavesCount;

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
			if (_leavesRemaining == 2)
			{
				if (_spriteRenderer.sprite != MatureFullLeavesSprite)
				{
					_spriteRenderer.sprite = MatureFullLeavesSprite;
				}
			}
			else if (_leavesRemaining == 1)
			{
				if (_spriteRenderer.sprite != MatureHalfLeafSprite)
				{
					_spriteRenderer.sprite = MatureHalfLeafSprite;
				}
			}
			else if (_leavesRemaining == 0)
			{
				if (_spriteRenderer.sprite != MatureSprite)
				{
					_spriteRenderer.sprite = MatureSprite;
				}
			}
			else // 에러!
			{
				Debug.LogWarning($"잘못된 잎 개수: {_leavesRemaining}");
				_leavesRemaining = 2;
			}
		}
		else if (growthSeconds >= Stage1GrowthSeconds) // 성장 2단계
		{
			if (!_trellisPlaced) // 짧은 지지대 설치 이전 상태
			{
				if (_spriteRenderer.sprite != BeforeTrellisSprite)
				{
					_spriteRenderer.sprite = BeforeTrellisSprite;
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


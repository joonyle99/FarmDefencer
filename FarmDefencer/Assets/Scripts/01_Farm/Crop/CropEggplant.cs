using UnityEngine;

public class CropEggplant : Crop
{
	public float DoubleTapCriterion = 0.5f;
	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	public float Stage1GrowthSeconds = 30.0f;
	[Space]
	public Sprite BeforeTrellisSprite;
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	[Space]
	public Sprite MatureFullLeavesSprite;
	public Sprite MatureHalfLeafSprite;
	public Sprite MatureSprite;
	public Sprite LockedSprite;
	public Sprite HarvestedSprite;
	[Space]
	public float NormalToDeadSeconds = 300.0f;
	public float DeadToSeedSeconds = 300.0f;

	private SpriteRenderer _spriteRenderer;
	private bool _trellisPlaced;
	private int _leavesRemaining;
	private float _lastSingleTapTime;

	public override void OnWatering()
	{
		if (State == CropState.Planted
			&& WaterWaitingSeconds < NormalToDeadSeconds + DeadToSeedSeconds
			&& WaterStored == 0.0f)
		{
			WaterStored += GrowthAgeSeconds >= Stage1GrowthSeconds ? (MatureAgeSeconds - Stage1GrowthSeconds) * 1.01f : Stage1GrowthSeconds * 1.01f;
		}
	}

	public override void OnSingleTap()
	{
		var currentTime = Time.time;
		if (_lastSingleTapTime + DoubleTapCriterion > currentTime)
		{
			if (GrowthAgeSeconds >= MatureAgeSeconds && _leavesRemaining > 0)
			{
				_leavesRemaining -= 1;
				_lastSingleTapTime = currentTime - DoubleTapCriterion; // 연속 입력 판정 방지
				return; // 액션 소모
			}
		}
		_lastSingleTapTime = currentTime;
		
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthAgeSeconds >= Stage1GrowthSeconds
				&& !_trellisPlaced)
			{
				_trellisPlaced = true;
			}
			if (IsHarvestable && _leavesRemaining == 0)
			{
				State = CropState.Harvested;
			}
		}
		else if (State == CropState.Harvested)
		{
			Itemify();
		}
	}

	protected override bool OnGrow(float deltaTime)
	{
		if (GrowthAgeSeconds >= Stage1GrowthSeconds && !_trellisPlaced) // 지지대 설치 이전에는 죽지 않음 일단
		{
			return false;
		}

		return true;
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		base.OnFarmUpdate(deltaTime);

		if (State == CropState.Seed)
		{
			_trellisPlaced = false;
			_spriteRenderer.sprite = SeedSprite;
			_leavesRemaining = 2;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthAgeSeconds >= MatureAgeSeconds) // 모두 성장한 단계
			{
				if (_leavesRemaining == 2)
				{
					_spriteRenderer.sprite = MatureFullLeavesSprite;
				}
				else if (_leavesRemaining == 1)
				{
					_spriteRenderer.sprite = MatureHalfLeafSprite;
				}
				else if (_leavesRemaining == 0)
				{
					_spriteRenderer.sprite = MatureSprite;
				}
				else // 에러!
				{
					Debug.LogWarning($"잘못된 잎 개수: {_leavesRemaining}");
					_leavesRemaining = 2;
				}
			}
			else if (GrowthAgeSeconds >= Stage1GrowthSeconds) // 성장 2단계
			{
				if (!_trellisPlaced) // 짧은 지지대 설치 이전 상태
				{
					_spriteRenderer.sprite = BeforeTrellisSprite;
				}
				else if (WaterStored == 0.0f) // 물 안준 상태
				{
					if (WaterWaitingSeconds >= NormalToDeadSeconds + DeadToSeedSeconds)
					{
						State = CropState.Locked;
					}
					else if (WaterWaitingSeconds >= NormalToDeadSeconds)
					{
						_spriteRenderer.sprite = Stage2DeadSprite;
					}
					else
					{
						_spriteRenderer.sprite = Stage2BeforeWateringSprite;
					}
				}
				else // 물 준 상태
				{
					_spriteRenderer.sprite = Stage2AfterWateringSprite;
				}
			}
			else // 성장 1단계
			{
				if (WaterStored == 0.0f) // 물 안준 상태
				{
					if (WaterWaitingSeconds >= NormalToDeadSeconds + DeadToSeedSeconds)
					{
						State = CropState.Locked;
					}
					else if (WaterWaitingSeconds >= NormalToDeadSeconds)
					{
						_spriteRenderer.sprite = Stage1DeadSprite;
					}
					else
					{
						_spriteRenderer.sprite = Stage1BeforeWateringSprite;
					}
				}
				else // 물 준 상태
				{
					_spriteRenderer.sprite = Stage1AfterWateringSprite;
				}
			}
		}
		else if (State == CropState.Harvested)
		{
			_spriteRenderer.sprite = HarvestedSprite;
		}
		else if (State == CropState.Locked)
		{
			_spriteRenderer.sprite = LockedSprite;
			if (LockRemainingSeconds == 0.0f)
			{
				State = CropState.Seed;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}
}


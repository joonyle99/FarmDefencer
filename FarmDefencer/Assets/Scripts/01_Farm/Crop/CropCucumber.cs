using UnityEngine; 

public class CropCucumber : Crop
{
	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	public float Stage1GrowthSeconds = 30.0f;
	[Space]
	public Sprite BeforeShortTrellisSprite;
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	[Space]
	public Sprite BeforeLongTrellisSprite;
	public Sprite MatureSprite;
	public Sprite LockedSprite;
	public Sprite HarvestedSprite;
	[Space]
	public float NormalToDeadSeconds = 300.0f;
	public float DeadToSeedSeconds = 300.0f;
	private SpriteRenderer _spriteRenderer;

	private bool _shortTrellisPlaced;
	private bool _longTrellisPlaced;

	public override void OnWatering()
	{
		if (State == CropState.Planted
			&& WaterWaitingSeconds < NormalToDeadSeconds + DeadToSeedSeconds
			&& WaterStored == 0.0f)
		{
			WaterStored += GrowthAgeSeconds >= Stage1GrowthSeconds ? (MatureAgeSeconds - Stage1GrowthSeconds) * 1.01f : Stage1GrowthSeconds * 1.01f;
			OnWatered.Invoke();
		}
	}

	public override void OnSingleTap()
	{
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
			OnPlanted.Invoke();
		}
		else if (State == CropState.Planted)
		{
			if (!_shortTrellisPlaced && !_longTrellisPlaced
				&& GrowthAgeSeconds >= Stage1GrowthSeconds)
			{
				_shortTrellisPlaced = true;
			}
			else if (_shortTrellisPlaced && !_longTrellisPlaced
				&& GrowthAgeSeconds >= MatureAgeSeconds)
			{
				_longTrellisPlaced = true;
			}
			else if (IsHarvestable)
			{
				State = CropState.Harvested;
				OnHarvested.Invoke();
			}
		}
		else if (State == CropState.Harvested && _shortTrellisPlaced && _longTrellisPlaced)
		{
			Itemify();
		}
	}

	protected override bool OnGrow(float deltaTime)
	{
		if (GrowthAgeSeconds >= Stage1GrowthSeconds && !_shortTrellisPlaced) // 짧은 지지대 설치 이전에는 죽지 않음 일단
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
			_shortTrellisPlaced = false;
			_longTrellisPlaced = false;
			_spriteRenderer.sprite = SeedSprite;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthAgeSeconds >= MatureAgeSeconds) // 모두 성장한 단계
			{
				if (GrowthAgeSeconds - deltaTime < MatureAgeSeconds) // 성장 직후 프레임이라면 물 초기화하기
				{
					WaterStored = 0.0f;
				}

				if (!_longTrellisPlaced) // 긴 지지대 설치 이전 상태
				{
					_spriteRenderer.sprite = BeforeLongTrellisSprite;
				}
				else
				{
					_spriteRenderer.sprite = MatureSprite;
				}
			}
			else if (GrowthAgeSeconds >= Stage1GrowthSeconds) // 성장 2단계
			{
				if (GrowthAgeSeconds - deltaTime < Stage1GrowthSeconds) // 성장 직후 프레임이라면 물 초기화하기
				{
					WaterStored = 0.0f;
				}

				if (!_shortTrellisPlaced) // 짧은 지지대 설치 이전 상태
				{
					_spriteRenderer.sprite = BeforeShortTrellisSprite;
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


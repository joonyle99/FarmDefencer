using UnityEngine;

public class CropCorn : Crop
{
	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	public float Stage1GrowthSeconds = 30.0f;
	[Space]
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	[Space]
	public Sprite MatureSprite;
	public Sprite LockedSprite;
	[Space]
	public float NormalToDeadSeconds = 300.0f;
	public float DeadToSeedSeconds = 300.0f;
	private SpriteRenderer _spriteRenderer;

	public override void OnTap()
	{
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Planted)
		{
			if (IsHarvestable)
			{
				State = CropState.Harvested;
			}
		}
		else if (State == CropState.Harvested)
		{
			OnHarvest();
			State = CropState.Seed;
		}
	}

	public override void OnWatering()
	{
		if (State == CropState.Planted 
			&& WaterWaitingSeconds < NormalToDeadSeconds + DeadToSeedSeconds
			&& WaterStored == 0.0f)
		{
			WaterStored += GrowthAgeSeconds >= Stage1GrowthSeconds ? (MatureAgeSeconds - Stage1GrowthSeconds) * 1.01f : Stage1GrowthSeconds * 1.01f;
		}
	}

	public override bool IsDead() => State == CropState.Planted && WaterWaitingSeconds >= NormalToDeadSeconds;
	public override bool IsGrowing() => State == CropState.Planted && !IsDead();

	protected override void Awake()
	{
		base.Awake();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	protected override void Update()
	{
		base.Update();

		if (State == CropState.Seed)
		{
			_spriteRenderer.sprite = SeedSprite;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthAgeSeconds >= MatureAgeSeconds) // 모두 성장한 단계
			{
				_spriteRenderer.sprite = MatureSprite;
			}
			else if (GrowthAgeSeconds >= Stage1GrowthSeconds) // 성장 2단계
			{
				if (WaterStored == 0.0f) // 물 안준 상태
				{
					if (WaterWaitingSeconds >= NormalToDeadSeconds + DeadToSeedSeconds)
					{
						State = CropState.Seed;
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
			_spriteRenderer.sprite = ProductEntry.ProductSprite;
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
}

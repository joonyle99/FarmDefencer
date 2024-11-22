using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropCarrot : Crop
{
	public Sprite SeedSprite;
	public Sprite JustPlantedSprite;
	public Sprite DeadSprite;
	public Sprite GrowingSprite;
	public Sprite BudSprite;
	public Sprite LockedSprite;
	[Space]
	public float PlantToDeadSeconds = 300.0f;
	public float DeadToSeedSeconds = 300.0f;
	private SpriteRenderer _spriteRenderer;

	public override void OnTap()
	{
		if (Paused)
		{
			return;
		}

		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthPercentage >= 100.0f)
			{
				State = CropState.Harvested;
			}
		}
		else if (State == CropState.Harvested)
		{
			Itemify();
		}
	}

	public override void OnWatering()
	{
		if (Paused)
		{
			return;
		}

		if (State != CropState.Planted)
		{
			return;
		}
		if (WaterWaitingSeconds < PlantToDeadSeconds + DeadToSeedSeconds && WaterStored == 0.0f)
		{
			WaterStored += MatureAgeSeconds * 1.1f; // 딱 맞아떨어지게 하면 99%에서 물 다시 달라고 할 수 있음
		}
	}

	public override bool IsDead() => State == CropState.Planted && WaterWaitingSeconds >= PlantToDeadSeconds;
	public override bool IsGrowing() => State == CropState.Planted && !IsDead();


	public override void OnFarmUpdate(float deltaTime)
	{
		base.OnFarmUpdate(deltaTime);

		if (State == CropState.Seed)
		{
			_spriteRenderer.sprite = SeedSprite;
		}
		else if (State == CropState.Planted)
		{
			if (GrowthPercentage >= 100.0f)
			{
				_spriteRenderer.sprite = BudSprite;
				return;
			}

			if (WaterStored > 0.0f)
			{
				_spriteRenderer.sprite = GrowingSprite;
			}
			else
			{
				if (WaterWaitingSeconds >= PlantToDeadSeconds+DeadToSeedSeconds)
				{
					State = CropState.Locked;
				}
				else if (WaterWaitingSeconds >= PlantToDeadSeconds)
				{
					_spriteRenderer.sprite = DeadSprite;
				}
				else
				{
					_spriteRenderer.sprite = JustPlantedSprite;
				}
			}
		}
		else if (State == CropState.Harvested)
		{
			_spriteRenderer.sprite = ProductEntry.ProductSprite;
		}
		else // Locked
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

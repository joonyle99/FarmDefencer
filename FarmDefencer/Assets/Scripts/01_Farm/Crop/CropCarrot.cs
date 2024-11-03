using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropCarrot : Crop
{
	public Sprite SeedSprite;
	public Sprite JustPlantedSprite;
	public Sprite DeadSprite;
	public Sprite GrowingSprite;
	public Sprite BudSprite;
	[Space]
	public float PlantToDeadSeconds = 300.0f;
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
			if (GrowthPercentage >= 100.0f)
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
		if (State != CropState.Planted)
		{
			return;
		}
		if (WaterWaitingSeconds < PlantToDeadSeconds && WaterStored == 0.0f)
		{
			WaterStored += MatureAgeSeconds * 1.1f; // 딱 맞아떨어지게 하면 99%에서 물 다시 달라고 할 수 있음
		}
	}

	public override bool IsDead() => State == CropState.Planted && WaterWaitingSeconds >= PlantToDeadSeconds;
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
					State = CropState.Seed;
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
		else // State == CropState.Harvested
		{
			_spriteRenderer.sprite = ProductEntry.ProductSprite;
		}
	}
}

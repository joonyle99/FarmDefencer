using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropCabbage : Crop
{
	public float DeltaShakeCriterion = 100.0f;
	public int ShakeCountCriterion = 4;

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
	public Sprite LockedSprite;
	public Sprite HarvestedSprite;
	[Space]
	public float NormalToDeadSeconds = 300.0f;
	public float DeadToSeedSeconds = 300.0f;
	private SpriteRenderer _spriteRenderer;
	private GameObject _harvestSoilLayerObject;
	private GameObject _harvestCropLayerObject;

	private int _shakeCount;
	private bool _wasLastShakeLeft;

	public override void OnSingleTap()
	{
		if (State == CropState.Seed)
		{
			State = CropState.Planted;
		}
		else if (State == CropState.Harvested)
		{
			Itemify();
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

	public override void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
	{
		if (isEnd)
		{
			ShakeMotion(0.0f);
			_shakeCount = 0;
			return;
		}

		if (State == CropState.Planted
			&& GrowthPercentage >= 100.0f)
		{
			var deltaShake = deltaPosition.x;
			ShakeMotion(deltaShake);
			if (Mathf.Abs(deltaShake) > DeltaShakeCriterion)
			{
				if (_shakeCount == 0)
				{
					_shakeCount += 1;
					_wasLastShakeLeft = deltaShake < 0.0f;
				}
				else
				{
					if (deltaShake < 0.0f && !_wasLastShakeLeft
						|| deltaShake > 0.0f && _wasLastShakeLeft)
					{
						_shakeCount += 1;
						_wasLastShakeLeft = !_wasLastShakeLeft;
					}
				}

				if (_shakeCount >= ShakeCountCriterion)
				{
					State = CropState.Harvested;
				}
			}
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		base.OnFarmUpdate(deltaTime);
		_harvestSoilLayerObject.SetActive(false);
		_harvestCropLayerObject.SetActive(false);

		if (State == CropState.Seed)
		{
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
				_harvestSoilLayerObject.SetActive(true);
				_harvestCropLayerObject.SetActive(true);
				_spriteRenderer.sprite = null;
			}
			else if (GrowthAgeSeconds >= Stage1GrowthSeconds) // 성장 2단계
			{
				if (GrowthAgeSeconds - deltaTime < Stage1GrowthSeconds) // 성장 직후 프레임이라면 물 초기화하기
				{
					WaterStored = 0.0f;
				}
				if (WaterStored == 0.0f) // 물 안준 상태
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
		_harvestSoilLayerObject = transform.GetChild(0).gameObject;
		_harvestCropLayerObject = transform.GetChild(1).gameObject;
	}

	private void ShakeMotion(float deltaShake)
	{
		if (deltaShake != 0.0f)
		{
			Debug.Log(deltaShake);
		}
		_harvestCropLayerObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Atan(-deltaShake) * 30.0f / Mathf.PI);
	}
}

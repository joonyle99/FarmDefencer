using UnityEngine;

/// <summary>
/// 0번 자식에 수확 단계 흙을 표시하기 위한 SpriteRenderer 오브젝트를 할당하고,
/// 1번 자식에 수확 단계 작물을 표시하기 위한 SpriteRenderer 오브젝트를 할당할 것.
/// </summary>
public class CropCabbage : Crop
{
	private const float DeltaShakeCriterion = 5.0f; // 배추 수확단계 흔들기 기준 (가로 방향 위치 델타)
	private const int ShakeCountCriterion = 4; // 배추 수확 흔들기 횟수 기준
	private const float Stage1GrowthSeconds = 10.0f;
	private const float Stage2GrowthSeconds = 10.0f;

	public Sprite SeedSprite;
	[Space]
	public Sprite Stage1BeforeWateringSprite;
	public Sprite Stage1DeadSprite;
	public Sprite Stage1AfterWateringSprite;
	[Space]
	public Sprite Stage2BeforeWateringSprite;
	public Sprite Stage2DeadSprite;
	public Sprite Stage2AfterWateringSprite;
	[Space]
	public Sprite HarvestedSprite;
	[Space]
	private SpriteRenderer _spriteRenderer; // 수확 단계를 제외한 모든 단계의 스프라이트 표시
	private GameObject _harvestSoilLayerObject; // 0번 자식
	private GameObject _harvestCropLayerObject; // 1번 자식

	private int _shakeCount;
	private bool _wasLastShakeLeft;
	private bool _isSeed;
	private bool _harvested;

	public override void OnSingleTap()
	{
		if (_isSeed)
		{
			_isSeed = false;
			SoundManager.PlaySfx("SFX_plant_seed");
		}
		else if (_harvested)
		{
			if (HarvestHandler(1) > 0)
			{
				_isSeed = true;
			}
		}
	}

	public override void OnWatering()
	{
		if (!_isSeed && !watered)
		{
			watered = true;
			waterWaitingSeconds = 0.0f;
			SoundManager.PlaySfx("SFX_water_oneshot");
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

		if (growthSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds
			&& !_harvested)
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
					_harvested = true;
					SoundManager.PlaySfx("SFX_harvest");
				}
			}
		}
	}

	public override void OnFarmUpdate(float deltaTime)
	{
		_harvestSoilLayerObject.SetActive(false);
		_harvestCropLayerObject.SetActive(false);

		if (_isSeed)
		{
			watered = false;
			waterWaitingSeconds = 0.0f;
			growthSeconds = 0.0f;
			_harvested = false;
			_wasLastShakeLeft = false;
			_shakeCount = 0;

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
			_harvestSoilLayerObject.SetActive(true);
			_harvestCropLayerObject.SetActive(true);
			_spriteRenderer.sprite = null;
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
		_harvestSoilLayerObject = transform.GetChild(0).gameObject;
		_harvestCropLayerObject = transform.GetChild(1).gameObject;
	}

	private void ShakeMotion(float deltaShake)
	{
		_harvestCropLayerObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Atan(-deltaShake) * 20.0f / Mathf.PI);
	}
}

//using UnityEngine;

//public class CropMushroom : Crop
//{
//	public Sprite Stage0Sprite;
//	public Sprite Stage1BeforeWateringSprite;
//	public Sprite Stage1DeadSprite;
//	public Sprite Stage1AfterWateringSprite;
//	public float Stage1GrowthSeconds = 20.0f;
//	[Space]
//	public Sprite Stage2BeforeWateringSprite;
//	public Sprite Stage2DeadSprite;
//	public Sprite Stage2AfterWateringSprite;
//	public float Stage2GrowthSeconds = 20.0f;
//	[Space]
//	public Sprite Stage3BeforeInoculationSprite;
//	public Sprite Stage3AfterInoculationSprite;
//	public float Stage3GrowthSeconds = 10.0f;
//	private bool _inoculated;
//	[Space]
//	public Sprite Stage4NormalSprite;
//	public Sprite Stage4PoisonousSprite;
//	[Space]
//	public Sprite LockedSprite;
//	public Sprite HarvestedSprite;
//	[Space]
//	public float NormalToDeadSeconds = 300.0f;
//	public float DeadToSeedSeconds = 300.0f;
//	private SpriteRenderer _spriteRenderer;
//	private float _holdingTime;
//	private bool _poisonousDetermined;
//	private bool _poisonous;
//	public override void OnSingleTap()
//	{
//		if (State == CropState.Planted)
//		{
//			if (IsHarvestable)
//			{
//				State = CropState.Harvested;
//				if (!_poisonous)
//				{
//					OnHarvested.Invoke();
//				}
//			}
//		}
//		else if (State == CropState.Harvested)
//		{
//			Itemify(_poisonous ? 0 : 1);
//		}
//	}

//	public override void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime)
//	{
//		if (isEnd)
//		{
//			_holdingTime = 0.0f;
//		}

//		if (State == CropState.Seed)
//		{
//			var deltaX = deltaPosition.x;
//			if (Mathf.Abs(deltaX) > 100.0f)
//			{
//				State = CropState.Planted;
//				OnPlanted.Invoke();
//			}
//		}
//		else if (State == CropState.Planted
//			&& GrowthAgeSeconds >= 40.0f
//			&& !_inoculated)
//		{
//			_holdingTime += deltaHoldTime;
//			if (_holdingTime >= 2.0f)
//			{
//				_inoculated = true;
//				_holdingTime = 0.0f;
//				WaterStored = 10.1f;
//			}
//		}
//	}

//	public override void OnWatering()
//	{
//		if (State == CropState.Planted
//			&& WaterWaitingSeconds < NormalToDeadSeconds + DeadToSeedSeconds
//			&& WaterStored == 0.0f)
//		{
//			WaterStored += GrowthAgeSeconds >= Stage1GrowthSeconds ? (MatureAgeSeconds - Stage1GrowthSeconds) * 1.01f : Stage1GrowthSeconds * 1.01f;
//			OnWatered.Invoke();
//		}
//	}
//	protected override bool OnGrow(float deltaTime)
//	{
//		if (GrowthAgeSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds && !_inoculated)
//		{
//			return false;
//		}

//		return true;
//	}

//	public override void OnFarmUpdate(float deltaTime)
//	{
//		base.OnFarmUpdate(deltaTime);

//		if (State == CropState.Seed)
//		{
//			_inoculated = false;
//			_poisonousDetermined = false;
//			_spriteRenderer.sprite = Stage0Sprite;
//		}
//		else if (State == CropState.Planted)
//		{
//			if (GrowthAgeSeconds >= MatureAgeSeconds) // 모두 성장한 단계
//			{
//				if (!_poisonousDetermined)
//				{
//					_poisonousDetermined = true;
//					_poisonous = Random.Range(0.0f, 1.0f) <= 0.65f;
//					_spriteRenderer.sprite = _poisonous ? Stage4PoisonousSprite : Stage4NormalSprite;
//				}
//			}
//			else if (GrowthAgeSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 성장 3단계
//			{
//				if (_inoculated)
//				{
//					_spriteRenderer.sprite = Stage3AfterInoculationSprite;
//				}
//				else
//				{
//					_spriteRenderer.sprite = Stage3BeforeInoculationSprite;
//				}
//			}
//			else if (GrowthAgeSeconds >= Stage1GrowthSeconds) // 성장 2단계
//			{
//				if (GrowthAgeSeconds - deltaTime < Stage1GrowthSeconds) // 성장 직후 프레임이라면 물 초기화하기
//				{
//					WaterStored = 0.0f;
//				}
//				if (WaterStored == 0.0f) // 물 안준 상태
//				{
//					if (WaterWaitingSeconds >= NormalToDeadSeconds + DeadToSeedSeconds)
//					{
//						State = CropState.Locked;
//					}
//					else if (WaterWaitingSeconds >= NormalToDeadSeconds)
//					{
//						_spriteRenderer.sprite = Stage2DeadSprite;
//					}
//					else
//					{
//						_spriteRenderer.sprite = Stage2BeforeWateringSprite;
//					}
//				}
//				else // 물 준 상태
//				{
//					_spriteRenderer.sprite = Stage2AfterWateringSprite;
//				}
//			}
//			else // 성장 1단계
//			{
//				if (WaterStored == 0.0f) // 물 안준 상태
//				{
//					if (WaterWaitingSeconds >= NormalToDeadSeconds + DeadToSeedSeconds)
//					{
//						State = CropState.Locked;
//					}
//					else if (WaterWaitingSeconds >= NormalToDeadSeconds)
//					{
//						_spriteRenderer.sprite = Stage1DeadSprite;
//					}
//					else
//					{
//						_spriteRenderer.sprite = Stage1BeforeWateringSprite;
//					}
//				}
//				else // 물 준 상태
//				{
//					_spriteRenderer.sprite = Stage1AfterWateringSprite;
//				}
//			}
//		}
//		else if (State == CropState.Harvested)
//		{
//			if (_poisonous)
//			{
//				State = CropState.Seed;
//			}
//			else
//			{
//				_spriteRenderer.sprite = HarvestedSprite;
//			}
//		}
//		else if (State == CropState.Locked)
//		{
//			_spriteRenderer.sprite = LockedSprite;
//			if (LockRemainingSeconds == 0.0f)
//			{
//				State = CropState.Seed;
//			}
//		}
//	}

//	protected override void Awake()
//	{
//		base.Awake();
//		_spriteRenderer = GetComponent<SpriteRenderer>();
//	}
//}

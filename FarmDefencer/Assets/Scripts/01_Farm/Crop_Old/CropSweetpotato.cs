//using UnityEngine;

//public class CropSweetpotato : Crop
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
//	public Sprite Stage3BeforeWrapSprite;
//	public Sprite Stage3AfterWrapSprite;
//	public float Stage3GrowthSeconds = 10.0f;
//	private bool _wrapped = false;
//	[Space]
//	public Sprite Stage4Sprite;
//	public float Stage4GrowthSeconds = 5.0f;
//	[Space]
//	public Sprite Stage5_X_Sprite;
//	public Sprite Stage5_O_Sprite;
//	[Space]
//	public Sprite Stage5_OO_Sprite;
//	public Sprite Stage5_OX_Sprite;
//	public Sprite Stage5_XO_Sprite;
//	public Sprite Stage5_XX_Sprite;
//	[Space]
//	public Sprite Stage5_XXX_Sprite;
//	public Sprite Stage5_XXO_Sprite;
//	public Sprite Stage5_XOX_Sprite;
//	public Sprite Stage5_OXX_Sprite;
//	public Sprite Stage5_XOO_Sprite;
//	public Sprite Stage5_OXO_Sprite;
//	public Sprite Stage5_OOX_Sprite;
//	public Sprite Stage5_OOO_Sprite;
//	[Space]
//	public Sprite LockedSprite;
//	public Sprite Harvested1Sprite;
//	public Sprite Harvested2Sprite;
//	public Sprite Harvested3Sprite;
//	private int _harvestedCount;
//	[Space]
//	public float NormalToDeadSeconds = 300.0f;
//	public float DeadToSeedSeconds = 300.0f;
//	private SpriteRenderer _spriteRenderer;
//	private float _lastSingleTapTime;
//	private int _tapCount;
//	private float _holdingTime;
//	private bool _countDetermined;
//	public override void OnSingleTap()
//	{
//		if (State == CropState.Planted)
//		{
//			if (IsHarvestable)
//			{
//				var currentTime = Time.time;
//				if (_lastSingleTapTime + 0.3f > currentTime)
//				{
//					_tapCount++;
//				}
//				else
//				{
//					_tapCount = 1;
//				}
//				_lastSingleTapTime = currentTime;
//				if (_tapCount >= 5)
//				{
//					State = CropState.Harvested;
//					if (_harvestedCount > 0)
//					{
//						OnHarvested.Invoke();
//					}
//				}
//			}
//		}
//		else if (State == CropState.Harvested)
//		{
//			Itemify(_harvestedCount);
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
//			&& !_wrapped)
//		{
//			_holdingTime += deltaHoldTime;
//			if (_holdingTime >= 2.0f)
//			{
//				_wrapped = true;
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
//		if (GrowthAgeSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds && !_wrapped) 
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
//			_wrapped = false;
//			_tapCount = 1;
//			_countDetermined = false;
//			_spriteRenderer.sprite = Stage0Sprite;
//		}
//		else if (State == CropState.Planted)
//		{
//			if (GrowthAgeSeconds >= MatureAgeSeconds) // 모두 성장한 단계
//			{
//				if (!_countDetermined)
//				{
//					_countDetermined = true;
//					var countDecision = Random.Range(0.0f, 1.0f);

//					if (countDecision >= 0.0f && countDecision < 0.6f)
//					{
//						var rotten = Random.Range(0.0f, 1.0f) < 0.2f;
//						if (rotten)
//						{
//							_harvestedCount = 0;
//							_spriteRenderer.sprite = Stage5_X_Sprite;
//						}
//						else
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_O_Sprite;
//						}
//					}
//					else if (countDecision >= 0.6f && countDecision < 0.9f)
//					{
//						var firstRotten = Random.Range(0.0f, 1.0f) < 0.3f;
//						var secondRotten = Random.Range(0.0f, 1.0f) < 0.3f;
//						if (firstRotten && secondRotten)
//						{
//							_harvestedCount = 0;
//							_spriteRenderer.sprite = Stage5_XX_Sprite;
//						}
//						else if (!firstRotten && secondRotten)
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_OX_Sprite;
//						}
//						else if (firstRotten && !secondRotten)
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_XO_Sprite;
//						}
//						else
//						{
//							_harvestedCount = 2;
//							_spriteRenderer.sprite = Stage5_OO_Sprite;
//						}
//					}
//					else
//					{
//						var firstRotten = Random.Range(0.0f, 1.0f) < 0.4f;
//						var secondRotten = Random.Range(0.0f, 1.0f) < 0.4f;
//						var thirdRotten = Random.Range(0.0f, 1.0f) < 0.4f;
//						if (firstRotten && secondRotten && thirdRotten)
//						{
//							_harvestedCount = 0;
//							_spriteRenderer.sprite = Stage5_XXX_Sprite;
//						}
//						else if (!firstRotten && secondRotten && thirdRotten)
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_OXX_Sprite;
//						}
//						else if (firstRotten && !secondRotten && thirdRotten)
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_XOX_Sprite;
//						}
//						else if (firstRotten && secondRotten && !thirdRotten)
//						{
//							_harvestedCount = 1;
//							_spriteRenderer.sprite = Stage5_XXO_Sprite;
//						}
//						else if (firstRotten && !secondRotten && !thirdRotten)
//						{
//							_harvestedCount = 2;
//							_spriteRenderer.sprite = Stage5_XOO_Sprite;
//						}
//						else if (!firstRotten && secondRotten && !thirdRotten)
//						{
//							_harvestedCount = 2;
//							_spriteRenderer.sprite = Stage5_OXO_Sprite;
//						}
//						else if (!firstRotten && !secondRotten && thirdRotten)
//						{
//							_harvestedCount = 2;
//							_spriteRenderer.sprite = Stage5_OOX_Sprite;
//						}
//						else
//						{
//							_harvestedCount = 3;
//							_spriteRenderer.sprite = Stage5_OOO_Sprite;
//						}
//					}
//				}
//			}
//			else if (GrowthAgeSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds + Stage3GrowthSeconds) // 성장 4단계
//			{
//				WaterStored = 10.0f; // 계속 성장하므로
//				_spriteRenderer.sprite = Stage4Sprite;
//			}
//			else if (GrowthAgeSeconds >= Stage1GrowthSeconds + Stage2GrowthSeconds) // 성장 3단계
//			{
//				if (_wrapped)
//				{
//					_spriteRenderer.sprite = Stage3AfterWrapSprite;
//				}
//				else
//				{
//					_spriteRenderer.sprite = Stage3BeforeWrapSprite;
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
//			if (_harvestedCount == 0)
//			{
//				State = CropState.Seed;
//			}
//			else
//			{ 	
//				if (_harvestedCount == 1)
//				{
//					_spriteRenderer.sprite = Harvested1Sprite;
//				}
//				else if (_harvestedCount == 2)
//				{
//					_spriteRenderer.sprite = Harvested2Sprite;
//				}
//				else // 3
//				{
//					_spriteRenderer.sprite = Harvested3Sprite;
//				}
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

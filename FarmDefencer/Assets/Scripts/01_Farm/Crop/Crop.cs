using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 작물 한 종류의 성장을 담당하는 추상 클래스입니다. 성장 동작과, 위치, 나이, 물과 관련한 상태 그리고 기본적인 설정값만이 존재합니다.
/// 각 작물은 이 클래스를 상속받아 시각적 표현, 성장 조건, 터치 등의 상호 작용을 정의해야 합니다.
/// <br/><br/>
/// <b>구체적인 설명:</b> 이 클래스는 OnGrow()가 true를 반환하고, CropState가 Growing이며, WaterStored > 0.0f인 상황이라면 MatureAgeSeconds까지 GrowthAgeSeconds를 증가시키는 동작을 합니다. <br/><br/>
/// 상속하는 클래스는 다음을 구현해야 할 것입니다: <br/>
/// <list type="bullet">물을 제외한 성장 조건이 있을 경우 OnGrow()를 오버라이드</list>
/// <list type="bullet">Update()에서 작물의 시각적 표현을 상태에 따라 업데이트</list>
/// <list type="bullet">작물이 죽을 조건이라면 State를 Seed로 조작(Crop은 작물의 State를 변경시키지 않음)</list>
/// <list type="bullet">OnTap(), OnHoldAndUp()을 오버라이드해 작물 심기, 수확 등의 동작 정의</list>
/// </summary>
public abstract class Crop : MonoBehaviour, IFarmUpdatable
{
	public ProductEntry ProductEntry;

	private UnityAction<UnityAction<bool>> _onTryItemifyAction;
	public CropState State
	{
		get
		{
			return _state;
		}
		protected set
		{
			if (_state == value)
			{
				return;
			}
			if (value == CropState.Seed
				|| value == CropState.Planted)
			{
				_growthAgeSeconds = 0.0f;
				_waterStored = 0.0f;
				_waterWaitingSeconds = 0.0f;
				_lockRemainingSeconds = 0.0f;
			}
			else if (value == CropState.Harvested)
			{
				_lockRemainingSeconds = 0.0f;
				_growthAgeSeconds = MatureAgeSeconds;
			}
			else if (value == CropState.Locked)
			{
				_lockRemainingSeconds = LockSeconds;
			}
			_state = value;
		}
	}

	[SerializeField]
	private CropState _state;
	/// <summary>
	/// 작물의 수확 가능한 상태까지 필요한 성장 시간 (GrowthAgeSeconds가 이 이상이어야 함)
	/// </summary>
	public float MatureAgeSeconds;
	/// <summary>
	/// 작물이 정상적으로 성장한 총 나이 (물이 없는 상태 등에서는 시간이 지나지 않음)
	/// </summary>
	public float GrowthAgeSeconds
	{
		get
		{
			return _growthAgeSeconds;
		}
		private set
		{
			_growthAgeSeconds = value;
			if (_growthAgeSeconds >= MatureAgeSeconds)
			{
				_growthAgeSeconds = MatureAgeSeconds;
			}
		}
	}
	[SerializeField]
	private float _growthAgeSeconds;
	/// <summary>
	/// 작물의 패널티 시간 설정값
	/// </summary>
	public float LockSeconds = 60.0f;
	/// <summary>
	/// 작물의 남은 패널티 시간
	/// </summary>
	public float LockRemainingSeconds
	{
		get
		{
			return _lockRemainingSeconds;
		}
		private set
		{
			_lockRemainingSeconds = value;
			if (_lockRemainingSeconds < 0.0f)
			{
				_lockRemainingSeconds = 0.0f;
			}
		}
	}
	[SerializeField]
	private float _lockRemainingSeconds;
	/// <summary>
	/// 물 저장량 (모든 작물은 1.0초당 1.0의 물을 소비)
	/// 물 주는 횟수 등을 조정하려면 물을 주는 양을 다르게 하면 될 것
	/// </summary>
	public float WaterStored
	{
		get
		{
			return _waterStored;
		}
		set
		{
			_waterStored = value;
			if (_waterStored < 0.0f)
			{
				_waterStored = 0.0f;
			}
		}
	}
	[SerializeField]
	private float _waterStored;
	/// <summary>
	/// 마지막 OnFarmUpdate()가 0.0이었으면 true, 이외의 경우 false로 세팅됩니다.
	/// </summary>
	protected bool Paused { get; private set; }

	/// <summary>
	/// 물이 없어서 기다리고 있었던 총 시간.
	/// 물을 주고 나면(WaterStored += amount) 0.0으로 초기화됩니다.
	/// </summary>
	public float WaterWaitingSeconds => _waterWaitingSeconds;
	[SerializeField]
	private float _waterWaitingSeconds;
	/// <summary>
	/// 작물의 성장도를 백분율로 나타냅니다.
	/// 즉 10초 중 5초가 지났다면 50.0f입니다.
	/// </summary>
	public float GrowthPercentage => MatureAgeSeconds <= 0.0f ? 100.0f : GrowthAgeSeconds / MatureAgeSeconds * 100.0f;
	public bool IsHarvestable => GrowthPercentage == 100.0f && State == CropState.Planted;
	public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;
	/// <summary>
	/// 이 Crop의 월드 XY 좌표를 반환합니다. 타일이기 때문에 Vector2Int로 반환합니다.
	/// </summary>
	public Vector2Int Position => new Vector2Int((int)transform.position.x, (int)transform.position.y);
	private CropLockedDisplay _cropLockedDisplay;

	/// <summary>
	/// 이 작물이 짧게 터치되었을 때의 동작을 정의합니다.
	/// </summary>
	public virtual void OnSingleTap() {}

	/// <summary>
	/// 이 작물에 대한 Single Holding 동작을 정의합니다.
	/// </summary>
	/// <param name="deltaPosition">첫 홀드 위치와 현재 홀드 위치의 차이입니다.</param>
	/// <param name="isEnd">홀드가 종료되어 마지막 액션인 경우 true입니다.</param>
	/// <param name="holdTime"></param>
	public virtual void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime) {}
	
	/// <summary>
	/// 이 작물에 물뿌리개의 물 주기 판정이 가해졌을 때의 동작을 정의합니다.
	/// </summary>
	public virtual void OnWatering() {}

	/// <summary>
	/// 작물을 아이템화 시도합니다. State가 Harvested가 아니라면 아무 일도 하지 않습니다.
	/// OnTryItemify&lt;afterItemify&lt;bool&gt;&gt; 이벤트를 발생시키며, afterItemify 콜백으로는 bool이 true일 경우 작물을 씨앗 상태로 되돌리는 작업이 전달됩니다.
	/// </summary>
	protected void Itemify()
	{
		if (State != CropState.Harvested)
		{
			return;
		}

		_onTryItemifyAction(
		(isItemified) =>
		{
			if (isItemified)
			{
				State = CropState.Seed;
			}
		});
	}

	/// <summary>
	/// 작물의 성장 한 프레임당 호출(Crop.Update()에서)되며, 특수한 성장 조건을 검사하는 메소드입니다.
	/// 작물별 물을 제외한 성장 조건이 모두 다를 수 있는데, 이를 구현하기 위한 메소드입니다.
	/// <br/><br/><b>물 보유량은 여기서 검사하지 마세요.</b>
	/// <br/><br/>
	/// State가 Growing이 아니라면 호출되지 않습니다.
	/// </summary>
	/// <returns>조건이 충족되었다면 true</returns>
	protected virtual bool OnGrow(float deltaTime) => true;
	public abstract bool IsDead();
	public abstract bool IsGrowing();
	public override string ToString() => $"{this.GetType()} at {Position}, age {GrowthAgeSeconds}, water waiting {WaterWaitingSeconds}, state {State}";

	public void Init(GameObject cropLockedDisplayPrefab, UnityAction<UnityAction<bool>> onTryItemifyAction)
	{
		var cropLockedDisplayObject = Instantiate(cropLockedDisplayPrefab);
		cropLockedDisplayObject.transform.SetParent(transform, false);
		cropLockedDisplayObject.transform.localPosition = new Vector2(0.0f, 0.0f);

		_cropLockedDisplay = cropLockedDisplayObject.GetComponent<CropLockedDisplay>();
		_cropLockedDisplay.Crop = this;
		_onTryItemifyAction = onTryItemifyAction;
	}

	public enum CropState
	{
		Seed, // 터치하여 Planted 상태가 되길 기다리는 상태
		Planted, // 성장중이거나, 죽었거나 하는 상태
		Harvested, // 100% 성장한 작물을 터치한 상태, 다시 터치하여 상자로 옮겨지길 기다리는 상태
		Locked // 작물 죽음 패널티 등으로 심기 동작이 잠긴 상태
	}

	protected virtual void Awake() {}
	protected virtual void Start() {}
	
	public virtual void OnFarmUpdate(float deltaTime)
	{
		Paused = deltaTime == 0.0f;
		if (_cropLockedDisplay != null)
		{
			_cropLockedDisplay.OnFarmUpdate(deltaTime);
		}

		if (State == CropState.Locked)
		{
			LockRemainingSeconds -= deltaTime;
			return;
		}

		if (_waterStored < deltaTime)
		{
			_waterStored = 0.0f;
			_waterWaitingSeconds += deltaTime;
			return;
		}

		if (OnGrow(deltaTime))
		{
			GrowthAgeSeconds += deltaTime;
			WaterStored -= deltaTime;
		}
	}
}
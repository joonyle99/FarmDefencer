using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Spine.Unity;

/// <summary>
/// 물뿌리개입니다.
/// UI의 형태로 구현되며 스크린 좌표로 유저의 입력을 받고, 월드 좌표로 Farm 등과 상호작용합니다.
/// 한 타일에 WateringTime 이상 머물면 OnWatering 이벤트를 발생시키는 동작을 합니다.
/// </summary>
public class WateringCan : MonoBehaviour,
	IDragHandler,
	IPointerDownHandler,
	IPointerUpHandler,
	IPointerExitHandler
{
	[Header("물 주기 판정이 가해지는 watering 애니메이션에서의 시각")]
	public float WateringAnimationTimePoint = 0.5f;
	[Header("물 주기 판정시 액션")]
	public UnityEvent<Vector2Int> OnWatering;
	public Camera Camera;
	private bool _using;
	public bool Using
	{
		get
		{
			return _using;
		}
		private set
		{
			if (_using != value)
			{
				_using = value;
				OnUsingStateChanged();
			}
		}
	}

	private Vector2Int _currentTilePosition;
	private Vector2 _initialScreenLocalPosition; // 이 물뿌리개를 사용하지 않을 때 위치할 화면 위치. 에디터 상에서 놓은 위치를 기억해서 사용함.
	private RectTransform _rectTransform;
	private FarmClock _farmClock;

	[SpineAnimation]
	public string IdleAnimationName;	
	[SpineAnimation]
	public string WateringAnimationName;

	private SkeletonGraphic _skeletonGraphic;
	private Spine.AnimationState _spineAnimationState;
	private Spine.Skeleton _skeleton;
	private bool _isWateredThisTime; // 이벤트 중복 호출 방지용

	public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
	public void OnPointerUp(PointerEventData pointerEventData) => MoveToInitialPosition();
	public void OnPointerExit(PointerEventData pointerEventData) => MoveToInitialPosition();

	public void OnDrag(PointerEventData pointerEventData)
	{
		if (_farmClock.IsPaused)
		{
			if (Using)
			{
				MoveToInitialPosition();
			}
			return;
		}

		Using = true;
		// 물뿌리개 위치를 현재 커서 위치로 옮기기
		_rectTransform.position = pointerEventData.position;

		// 물뿌리개의 월드 위치 구하기
		var pointerScreenPosition = pointerEventData.position;
		var pointerWorldPosition = Camera.ScreenToWorldPoint(pointerScreenPosition);

		var currentRoundX = Mathf.RoundToInt(pointerWorldPosition.x);
		var currentRoundY = Mathf.RoundToInt(pointerWorldPosition.y);
		var newTilePosition = new Vector2Int(currentRoundX, currentRoundY);

		_currentTilePosition = newTilePosition;
	}

	public void Init(FarmClock farmClock)
	{
		_farmClock = farmClock;
	}

	private void Awake()
	{
		var tileX = Mathf.FloorToInt(transform.position.x);
		var tileY = Mathf.FloorToInt(transform.position.y);
		_rectTransform = GetComponent<RectTransform>();
		_initialScreenLocalPosition = _rectTransform.localPosition;

		_skeletonGraphic = GetComponent<SkeletonGraphic>();
		_spineAnimationState = _skeletonGraphic.AnimationState;
		_skeleton = _skeletonGraphic.Skeleton;
	}

	private void Update()
	{
		if (!Using)
		{
			return;
		}

		var currentTrackEntry = _spineAnimationState.GetCurrent(0);
		if (currentTrackEntry.Animation.Name == WateringAnimationName)
		{
			if (currentTrackEntry.AnimationTime < WateringAnimationTimePoint)
			{
				_isWateredThisTime = false;
			}
			else
			{
				if (!_isWateredThisTime)
				{
					_isWateredThisTime = true;
					OnWatering.Invoke(_currentTilePosition);
				}
			}
		}
	}

	private void MoveToInitialPosition()
	{
		_rectTransform.localPosition = _initialScreenLocalPosition;
		Using = false;
	}

	private void OnUsingStateChanged()
	{
		if (_using)
		{
			_spineAnimationState.SetAnimation(0, WateringAnimationName, true);
		}
		else
		{
			_spineAnimationState.SetAnimation(0, IdleAnimationName, true).MixDuration = 0.0f;
		}
	}
}

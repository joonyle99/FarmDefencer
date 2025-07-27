using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// UI로 구현되는 물뿌리개.
/// FarmInput과는 별개로 동작하며, 반드시 main camera가 설정되어 있어야 함.
/// </summary>
public sealed class WateringCan :
	MonoBehaviour,
	IFarmInputLayer,
	IDragHandler,
	IPointerDownHandler,
	IPointerUpHandler,
	IPointerExitHandler
{
	[InfoBox("실제 물주기 판정이 판정이 발생할 위치의 물뿌리개 몸체로부터의 오프셋. 화면 해상도 기준.")]
	[SerializeField] private Vector2 wateringOffset = new Vector2(512.0f, -512.0f);
	public Vector2 WateringOffset => wateringOffset;
	
	public int InputPriority => IFarmInputLayer.Priority_WateringCan;

	// 물 주기 판정이 가해지는 watering 애니메이션에서의 시각
	private const float WateringAnimationTimePoint = 0.5f;
	private Action<Vector2> _onWatering;
	public bool Using { get; private set; }

	private Vector2 _currentWateringWorldPosition;
	private Vector2 _initialScreenLocalPosition; // 이 물뿌리개를 사용하지 않을 때 위치할 화면 위치. 에디터 상에서 놓은 위치를 기억해서 사용함.
	private RectTransform _rectTransform;
	private Func<bool> _isWaterable;

	[SpineAnimation]
	[SerializeField]
	private string idleAnimationName;	
	[SpineAnimation]
	[SerializeField]
	private string wateringAnimationName;

	private SkeletonGraphic _skeletonGraphic;
	private Spine.AnimationState _spineAnimationState;
	private bool _isWateredThisTime; // 이벤트 중복 호출 방지용

	public bool OnTap(Vector2 worldPosition)
	{
		return false;
	}

	public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		return Using;
	}

	public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
	public void OnPointerUp(PointerEventData pointerEventData) => MoveToInitialPosition();

	public void OnPointerExit(PointerEventData pointerEventData)
	{

	}

	public void OnDrag(PointerEventData pointerEventData)
	{
		if (!_isWaterable())
		{
			if (Using)
			{
				MoveToInitialPosition();
			}

			return;
		}

		if (!Using)
		{
			Using = true;
			OnUsingStateChanged();
		}

		// 물뿌리개 위치를 현재 커서 위치로 옮기기
		_rectTransform.anchoredPosition = pointerEventData.position;
		// 물뿌리개의 월드 위치 구하기
		var pointerScreenPosition = pointerEventData.position;
		var wateringScreenPosition = pointerScreenPosition + wateringOffset;
		_currentWateringWorldPosition = Camera.main.ScreenToWorldPoint(wateringScreenPosition);
	}

	public void Init(Func<bool> isWaterable, Action<Vector2> onWatering)
	{
		_isWaterable = isWaterable;
		_onWatering = onWatering;
	}

	private void Awake()
	{
		_rectTransform = transform.Find("Root").GetComponent<RectTransform>();
		_initialScreenLocalPosition = _rectTransform.localPosition;

		_skeletonGraphic = transform.Find("Root/Animation").GetComponent<SkeletonGraphic>();
		_spineAnimationState = _skeletonGraphic.AnimationState;
	}

	private void Update()
	{
		if (!Using)
		{
			return;
		}

		var currentTrackEntry = _spineAnimationState.GetCurrent(0);
		if (currentTrackEntry.Animation.Name != wateringAnimationName)
		{
			return;
		}

		if (currentTrackEntry.AnimationTime < WateringAnimationTimePoint)
		{
			_isWateredThisTime = false;
		}
		else if (!_isWateredThisTime)
		{
			_isWateredThisTime = true;
			_onWatering?.Invoke(_currentWateringWorldPosition);
		}
	}

	private void MoveToInitialPosition()
	{
		_rectTransform.localPosition = _initialScreenLocalPosition;
		Using = false;
		OnUsingStateChanged();
	}

	private void OnUsingStateChanged()
	{
		if (Using)
		{
			_spineAnimationState.SetAnimation(0, wateringAnimationName, true);
		}
		else
		{
			_spineAnimationState.SetAnimation(0, idleAnimationName, true).MixDuration = 0.0f;
		}
	}
}

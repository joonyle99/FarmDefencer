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
	[InfoBox("실제 물주기 판정이 판정이 발생할 위치의 물뿌리개 몸체로부터의 오프셋. 월드 좌표 기준.")]
	[SerializeField] private Vector2 wateringOffset = new(1.0f, -1.0f);
	
	public int InputPriority => IFarmInputLayer.Priority_WateringCan;

	// 물 주기 판정이 가해지는 watering 애니메이션에서의 시각
	private const float WateringAnimationTimePoint = 0.5f;
	private Action<Vector2> _onWatering;
	private bool _using;

	public bool Using
	{
		get => _using;
		private set
		{
			if (_using == value)
			{
				return;
			}
			_using = value;
			OnUsingStateChanged();
		}
	}

	private Func<bool> _isWaterable;

	[SpineAnimation]
	[SerializeField]
	private string idleAnimationName;	
	[SpineAnimation]
	[SerializeField]
	private string wateringAnimationName;

	private SkeletonAnimation _worldWateringCan;
	private SkeletonGraphic _uiWateringCan;

	public bool OnTap(Vector2 worldPosition)
	{
		return false;
	}

	public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		return Using;
	}

	public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
	public void OnPointerUp(PointerEventData pointerEventData) => Using = false;

	public void OnPointerExit(PointerEventData pointerEventData)
	{

	}

	public void OnDrag(PointerEventData pointerEventData)
	{
		if (!_isWaterable())
		{
			Using = false;
			return;
		}
		
		Using = true;
		
		// 물뿌리개의 월드 위치 구하기
		var pointerScreenPosition = pointerEventData.position;
		
		var worldWateringCanPosition = Camera.main.ScreenToWorldPoint(pointerScreenPosition);
		worldWateringCanPosition.z = -5.0f;
		_worldWateringCan.transform.position = worldWateringCanPosition;
	}

	public void Init(Func<bool> isWaterable, Action<Vector2> onWatering)
	{
		_isWaterable = isWaterable;
		_onWatering = onWatering;
	}

	private void Awake()
	{
		_uiWateringCan = transform.Find("UI/Animation").GetComponent<SkeletonGraphic>();
		_worldWateringCan = transform.Find("World").GetComponent<SkeletonAnimation>();
	}

	private void Start()
	{
		OnUsingStateChanged();
	}

	private void Update()
	{
		if (!Using)
		{
			return;
		}

		_onWatering?.Invoke(new Vector2(_worldWateringCan.transform.position.x + wateringOffset.x - 0.5f, _worldWateringCan.transform.position.y + wateringOffset.y - 0.5f));
		_onWatering?.Invoke(new Vector2(_worldWateringCan.transform.position.x + wateringOffset.x + 0.5f, _worldWateringCan.transform.position.y + wateringOffset.y - 0.5f));
		_onWatering?.Invoke(new Vector2(_worldWateringCan.transform.position.x + wateringOffset.x - 0.5f, _worldWateringCan.transform.position.y + wateringOffset.y + 0.5f));
		_onWatering?.Invoke(new Vector2(_worldWateringCan.transform.position.x + wateringOffset.x + 0.5f, _worldWateringCan.transform.position.y + wateringOffset.y + 0.5f));
	}

	private void OnUsingStateChanged()
	{
		_worldWateringCan.gameObject.SetActive(Using);
		_uiWateringCan.gameObject.SetActive(!Using);
		
		if (Using)
		{
			_worldWateringCan.AnimationState.SetAnimation(0, wateringAnimationName, true);
		}
		else
		{
			_worldWateringCan.AnimationState.SetAnimation(0, idleAnimationName, true).MixDuration = 0.0f;
		}
	}
}

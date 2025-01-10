using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Spine.Unity;

/// <summary>
/// ���Ѹ����Դϴ�.
/// UI�� ���·� �����Ǹ� ��ũ�� ��ǥ�� ������ �Է��� �ް�, ���� ��ǥ�� Farm ��� ��ȣ�ۿ��մϴ�.
/// �� Ÿ�Ͽ� WateringTime �̻� �ӹ��� OnWatering �̺�Ʈ�� �߻���Ű�� ������ �մϴ�.
/// </summary>
public class WateringCan : MonoBehaviour,
	IDragHandler,
	IPointerDownHandler,
	IPointerUpHandler,
	IPointerExitHandler
{
	[Header("�� �ֱ� ������ �������� watering �ִϸ��̼ǿ����� �ð�")]
	public float WateringAnimationTimePoint = 0.5f;
	[Header("�� �ֱ� ������ �׼�")]
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
	private Vector2 _initialScreenLocalPosition; // �� ���Ѹ����� ������� ���� �� ��ġ�� ȭ�� ��ġ. ������ �󿡼� ���� ��ġ�� ����ؼ� �����.
	private RectTransform _rectTransform;
	private FarmClock _farmClock;

	[SpineAnimation]
	public string IdleAnimationName;	
	[SpineAnimation]
	public string WateringAnimationName;

	private SkeletonGraphic _skeletonGraphic;
	private Spine.AnimationState _spineAnimationState;
	private Spine.Skeleton _skeleton;
	private bool _isWateredThisTime; // �̺�Ʈ �ߺ� ȣ�� ������

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
		// ���Ѹ��� ��ġ�� ���� Ŀ�� ��ġ�� �ű��
		_rectTransform.position = pointerEventData.position;

		// ���Ѹ����� ���� ��ġ ���ϱ�
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

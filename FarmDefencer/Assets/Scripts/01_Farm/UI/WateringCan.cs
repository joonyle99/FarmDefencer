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
	[Header("�� �ֱ� ���� �ð�")]
	public float WateringTime = 1.0f;
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
			_using = value;
			OnUsingStateChanged();
		}
	}

	private Vector2Int _currentTilePosition;
	private float _elapsedTileTime; // ���� Ÿ�Ͽ� �־��� �ð�
	private Vector2 _initialScreenLocalPosition; // �� ���Ѹ����� ������� ���� �� ��ġ�� ȭ�� ��ġ. ������ �󿡼� ���� ��ġ�� ����ؼ� �����.
	private RectTransform _rectTransform;

	[SpineAnimation]
	public string WateringAnimationName;

	private SkeletonGraphic _skeletonGraphic;
	private Spine.AnimationState _spineAnimationState;
	private Spine.Skeleton _skeleton;

	public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
	public void OnPointerUp(PointerEventData pointerEventData) => MoveToInitialPosition();
	public void OnPointerExit(PointerEventData pointerEventData) => MoveToInitialPosition();

	public void OnDrag(PointerEventData pointerEventData)
	{
		Using = true;
		// ���Ѹ��� ��ġ�� ���� Ŀ�� ��ġ�� �ű��
		_rectTransform.position = pointerEventData.position;

		// ���Ѹ����� ���� ��ġ ���ϱ�
		var pointerScreenPosition = pointerEventData.position;
		var pointerWorldPosition = Camera.ScreenToWorldPoint(pointerScreenPosition);

		var currentRoundX = Mathf.RoundToInt(pointerWorldPosition.x);
		var currentRoundY = Mathf.RoundToInt(pointerWorldPosition.y);
		var newTilePosition = new Vector2Int(currentRoundX, currentRoundY);

		if (newTilePosition != _currentTilePosition)
		{
			_elapsedTileTime = 0.0f;
		}
		_currentTilePosition = newTilePosition;
	}

	private void Awake()
	{
		var tileX = Mathf.FloorToInt(transform.position.x);
		var tileY = Mathf.FloorToInt(transform.position.y);
		_elapsedTileTime = 0.0f;
		_rectTransform = GetComponent<RectTransform>();
		_initialScreenLocalPosition = _rectTransform.localPosition;

		_skeletonGraphic = GetComponent<SkeletonGraphic>();
		_spineAnimationState = _skeletonGraphic.AnimationState;
		_skeleton = _skeletonGraphic.Skeleton;

		_spineAnimationState.SetEmptyAnimation(0, 0.1f);
	}

	private void Update()
	{
		if (!Using)
		{
			_elapsedTileTime = 0.0f;
			return;
		}
		_elapsedTileTime += Time.deltaTime;
		if (_elapsedTileTime >= WateringTime)
		{
			_elapsedTileTime = 0.0f;
			OnWatering.Invoke(_currentTilePosition);
		}
	}

	private void MoveToInitialPosition()
	{
		_rectTransform.localPosition = _initialScreenLocalPosition;
		_elapsedTileTime = 0.0f;
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
			_spineAnimationState.SetEmptyAnimation(0, 0.1f);
		}
	}
}

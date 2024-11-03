using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// ���Ѹ����Դϴ�.
/// UI�� ���·� �����Ǹ� ��ũ�� ��ǥ�� ������ �Է��� �ް�, ���� ��ǥ�� Farm ��� ��ȣ�ۿ��մϴ�.
/// �� Ÿ�Ͽ� WateringTime �̻� �ӹ��� OnWatering �̺�Ʈ�� �߻���Ű�� ������ �մϴ�.
/// </summary>
[RequireComponent(typeof(Image))]
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
			_image.sprite = _using ? WateringSprite : NormalSprite; 
		}
	}
	public Sprite WateringSprite;
	public Sprite NormalSprite;

	private Vector2Int _currentTilePosition; // OnDrag()���� ������Ʈ�� ���Ѹ��� Ÿ�� ��ġ
	private Vector2Int _lastTilePositon; // ���� �����ӿ� �� ���Ѹ����� �־��� Ÿ�� ��ġ
	private float _elapsedTileTime; // ���� Ÿ�Ͽ� �־��� �ð�
	private Vector2 _initialScreenLocalPosition; // �� ���Ѹ����� ������� ���� �� ��ġ�� ȭ�� ��ġ. ������ �󿡼� ���� ��ġ�� ����ؼ� �����.
	private RectTransform _rectTransform;
	private Image _image;

	public void OnPointerDown(PointerEventData eventData){ } // OnPointerUp ȣ�� �����ϰ� �Ϸ��� OnPointerDown�� �ʿ���
	public void OnPointerUp(PointerEventData pointerEventData) => MoveToInitialPosition();
	public void OnPointerExit(PointerEventData pointerEventData) => MoveToInitialPosition();

	public void OnDrag(PointerEventData pointerEventData)
	{
		Using = true;
		// ���Ѹ��� ��ġ�� ���� Ŀ�� ��ġ�� �ű��
		_rectTransform.position = pointerEventData.position;

		// ���Ѹ����� ���� ��ġ ���ϱ�.
		// pointerCurrentRaycast.worldPosition�� (0, 0, 0)�� ��ȯ�ؼ�, screenPosition�� ���� �� �̸� ��ȯ
		var pointerScreenPosition = pointerEventData.pointerCurrentRaycast.screenPosition;
		var pointerWorldPosition = Camera.ScreenToWorldPoint(pointerScreenPosition);

		var currentRoundX = Mathf.RoundToInt(pointerWorldPosition.x);
		var currentRoundY = Mathf.RoundToInt(pointerWorldPosition.y);
		_currentTilePosition = new Vector2Int(currentRoundX, currentRoundY);
	}

	private void Awake()
	{
		var tileX = Mathf.FloorToInt(transform.position.x);
		var tileY = Mathf.FloorToInt(transform.position.y);
		_lastTilePositon = new Vector2Int(tileX, tileY);
		_elapsedTileTime = 0.0f;
		_rectTransform = GetComponent<RectTransform>();
		_initialScreenLocalPosition = _rectTransform.localPosition;
		_image = GetComponent<Image>();
	}

	private void Update()
	{
		if (!Using)
		{
			return;
		}	

		if (_currentTilePosition == _lastTilePositon)
		{
			var deltaTime = Time.deltaTime;
			_elapsedTileTime += deltaTime;

			if (_elapsedTileTime >= WateringTime)
			{
				_elapsedTileTime = 0.0f;
				OnWatering.Invoke(_currentTilePosition);
			}
		}
		else
		{
			_elapsedTileTime = 0.0f;
		}
		_lastTilePositon = _currentTilePosition;
	}

	private void MoveToInitialPosition()
	{
		_rectTransform.localPosition = _initialScreenLocalPosition;
		_elapsedTileTime = 0.0f;
		Using = false;
	}
}

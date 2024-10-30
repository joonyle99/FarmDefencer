using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 물뿌리개입니다.
/// UI의 형태로 구현되며 스크린 좌표로 유저의 입력을 받고, 월드 좌표로 Farm 등과 상호작용합니다.
/// </summary>
[RequireComponent(typeof(Image))]
public class WateringCan : MonoBehaviour,
	IDragHandler,
	IPointerDownHandler,
	IPointerUpHandler,
	IPointerExitHandler
{
	[Header("물 주기 판정 시간")]
	public float WateringTime = 1.0f;
	[Header("물 주기 판정시 액션")]
	public UnityEvent<Vector2Int> OnWatering;
	public Camera Camera;

	private Vector2Int _currentTilePosition; // OnDrag()에서 업데이트한 물뿌리개 타일 위치
	private Vector2Int _lastTilePositon; // 이전 프레임에 이 물뿌리개가 있었던 타일 위치
	private float _elapsedTileTime; // 현재 타일에 있었던 시간
	private Vector2 _initialScreenLocalPosition; // 이 물뿌리개를 사용하지 않을 때 위치할 화면 위치. 에디터 상에서 놓은 위치를 기억해서 사용함.
	private RectTransform _rectTransform;
	private bool _isUsing;

	public void OnPointerDown(PointerEventData eventData){ } // OnPointerUp 호출 가능하게 하려면 OnPointerDown이 필요함
	public void OnPointerUp(PointerEventData pointerEventData) => MoveToInitialPosition();
	public void OnPointerExit(PointerEventData pointerEventData) => MoveToInitialPosition();

	public void OnDrag(PointerEventData pointerEventData)
	{
		_isUsing = true;
		// 물뿌리개 위치를 현재 커서 위치로 옮기기
		_rectTransform.position = pointerEventData.position;

		// 물뿌리개의 월드 위치 구하기.
		// pointerCurrentRaycast.worldPosition이 (0, 0, 0)을 반환해서, screenPosition을 얻은 후 이를 변환
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
	}

	private void Update()
	{
		if (!_isUsing)
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
		_isUsing = false;
	}
}

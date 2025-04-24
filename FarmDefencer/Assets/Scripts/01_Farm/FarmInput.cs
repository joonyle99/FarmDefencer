using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 타이쿤 씬에서 유저 입력을 받는 컴포넌트.
/// </summary>
public sealed class FarmInput : MonoBehaviour
{
	private const float MaximumCameraMovementScale = 10.0f;
	private const float MinimumCameraMovementScale = 0.1f;

	private const float MaximumProjectionSize = 10.0f; // 가장 넓게 볼 때의 크기를 의미.
	private const float MinimumProjectionSize = 5.0f; // 가장 좁게 볼 때의 크기를 의미.

	private const float MovableWidth = 15.0f;
	private const float MovableHeight = 10.0f;

	private float _cameraMovementScale = 0.1f;

	public float CameraMovementScale
	{
		get
		{
			return _cameraMovementScale;
		}
		set
		{
			if (value < MinimumCameraMovementScale || value > MaximumCameraMovementScale)
			{
				Debug.LogError($"FarmInput.CameraMovementScale은 {MinimumCameraMovementScale} 이상 {MaximumCameraMovementScale} 이하의 값을 가져야 합니다.");
			}
			else
			{
				_cameraMovementScale = value;
			}
		}
	}

	private Camera _camera;

	private bool _isSingleHolding;
	private float _singleHoldingTimeElapsed;
	private Vector2 _lastInteractedWorldPosition;
	private Vector2 _initialSingleHoldingWorldPosition;
	private List<IFarmInputLayer> _inputLayers;

	private float _zoomMomentum;

	public void FullZoomOut()
	{
		_camera.orthographicSize = MaximumProjectionSize;
	}

	public void RegisterInputLayer(IFarmInputLayer inputLayer)
	{
		_inputLayers.Add(inputLayer);
		_inputLayers.Sort((left, right) => right.InputPriority.CompareTo(left.InputPriority));
	}

	// 현재 터치된 월드 위치 또는 커서의 월드 위치를 설정함.
	private void OnInteract(InputValue inputValue)
	{
		_lastInteractedWorldPosition = _camera.ScreenToWorldPoint(inputValue.Get<Vector2>());
	}

	private void OnSingleTap()
	{
		foreach (var inputLayer in _inputLayers)
		{
			if (inputLayer.OnSingleTap(_lastInteractedWorldPosition))
			{
				break;
			}
		}
	}


	// 누르는 순간, 떼는 순간만 감지하기 때문에, 실제 호출은 Update()에서 진행.
	private void OnSingleHold(InputValue inputValue)
	{
		var isCurrentFrameHolding = inputValue.Get<float>() >= 0.5f;

		if (!_isSingleHolding && isCurrentFrameHolding)
		{
			_initialSingleHoldingWorldPosition = _lastInteractedWorldPosition;
		}

		_isSingleHolding = isCurrentFrameHolding;
	}

	private void OnMouseWheel(InputValue inputValue)
	{
		var input = inputValue.Get<float>();
		if (input != 0.0f)
		{
			_zoomMomentum = -1.0f * input;
		}
	}


	private void Update()
	{
		if (_isSingleHolding)
		{
			_singleHoldingTimeElapsed += Time.deltaTime;
			var deltaPosition = _lastInteractedWorldPosition - _initialSingleHoldingWorldPosition;

			bool isHandled = false;
			foreach (var inputLayer in _inputLayers)
			{
				if (inputLayer.OnSingleHolding(
					_initialSingleHoldingWorldPosition,
					deltaPosition,
					false,
					Time.deltaTime
				))
				{
					isHandled = true;
					break;
				}
			}

			if (!isHandled)
			{
				var cameraDelta = new Vector2(deltaPosition.x * -1.0f, deltaPosition.y * -1.0f);
				MoveCamera(new Vector2(transform.position.x, transform.position.y) + cameraDelta * (CameraMovementScale * _camera.orthographicSize));
			}
		}
		else if (_singleHoldingTimeElapsed > 0.0f) // Single Hold가 종료된 이후의 첫 프레임임을 의미.
		{
			_singleHoldingTimeElapsed = 0.0f;

			_inputLayers.ForEach(inputLayer => inputLayer.OnSingleHolding(
					_initialSingleHoldingWorldPosition,
					_lastInteractedWorldPosition - _initialSingleHoldingWorldPosition,
					true,
					Time.deltaTime
				));
		}

		var currentProjectionSize = _camera.orthographicSize;
		var newProjectionSize = Mathf.Clamp(currentProjectionSize + _zoomMomentum * 0.07f, MinimumProjectionSize, MaximumProjectionSize);
		_camera.orthographicSize = newProjectionSize;


		var dampedMomentum = _zoomMomentum < 0.0f ? Mathf.Min(_zoomMomentum + Time.deltaTime * 5.0f, 0.0f) : Mathf.Max(_zoomMomentum - Time.deltaTime * 5.0f, 0.0f);
		_zoomMomentum = dampedMomentum;
		MoveCamera(new Vector2(transform.position.x, transform.position.y));
	}

	private void Awake()
	{
		_inputLayers = new List<IFarmInputLayer>();
		_camera = GetComponent<Camera>();
		_camera.tag = "MainCamera";
	}

	private void MoveCamera(Vector2 worldPosition)
	{
		var newPosition = new Vector3(worldPosition.x, worldPosition.y, -10.0f);
		var movableMultiplier = (MaximumProjectionSize - _camera.orthographicSize) / (MaximumProjectionSize - MinimumProjectionSize);

		newPosition.x = Mathf.Clamp(newPosition.x, -MovableWidth * movableMultiplier, MovableWidth * movableMultiplier);
		newPosition.y = Mathf.Clamp(newPosition.y, -MovableHeight * movableMultiplier, MovableHeight * movableMultiplier);
		transform.position = newPosition;
	}
}

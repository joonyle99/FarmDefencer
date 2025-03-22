using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 타이쿤 씬에서 유저 입력을 받는 컴포넌트.
/// </summary>
public sealed class FarmInput : MonoBehaviour
{
	private const float MaximumCameraMovementScale = 10.0f;
	private const float MinimumCameraMovementScale = 0.01f;

	private const float MovableWidth = 10.0f;
	private const float MovableHeight = 10.0f;


	private float _cameraMovementScale = 0.05f;
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

	private bool _isDoubleHolding;
	private bool _isSingleHolding;
	private float _singleHoldingTimeElapsed;
	private Vector2 _lastInteractedWorldPosition;
	private Vector2 _initialSingleHoldingWorldPosition;
	private List<IFarmInputLayer> _inputLayers;

	public void RegisterInputLayer(IFarmInputLayer inputLayer)
	{
		_inputLayers.Add(inputLayer);
	}

	private void OnCameraMove(InputValue inputValue)
	{
		if (_isDoubleHolding)
		{
			var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;
			var newPosition = transform.position + (Vector3)scaledInputVector;
			newPosition.x = Mathf.Clamp(newPosition.x, -MovableWidth, MovableWidth);
			newPosition.y = Mathf.Clamp(newPosition.y, -MovableHeight, MovableHeight);
			newPosition.z = -10.0f;
			transform.position = newPosition;
		}
	}

	// 현재 터치된 월드 위치 또는 커서의 월드 위치를 설정함.
	private void OnInteract(InputValue inputValue)
	{
		_lastInteractedWorldPosition = _camera.ScreenToWorldPoint(inputValue.Get<Vector2>());
	}

	private void OnSingleTap() => _inputLayers.ForEach(inputLayer => inputLayer.OnSingleTap(_lastInteractedWorldPosition));

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

	private void OnDoubleHold(InputValue inputValue) => _isDoubleHolding = inputValue.Get<float>() >= 0.5f;


	private void Update()
	{
		if (_isSingleHolding)
		{
			_singleHoldingTimeElapsed += Time.deltaTime;

			_inputLayers.ForEach(inputLayer => inputLayer.OnSingleHolding(
					_initialSingleHoldingWorldPosition,
					_lastInteractedWorldPosition - _initialSingleHoldingWorldPosition,
					false,
					Time.deltaTime
				));
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
	}

	private void Awake()
	{
		_inputLayers = new List<IFarmInputLayer>();
		_camera = GetComponent<Camera>();
		_camera.tag = "MainCamera";
	}
}

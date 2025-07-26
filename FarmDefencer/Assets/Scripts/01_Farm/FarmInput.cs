using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

/// <summary>
/// 타이쿤 씬에서 유저 입력을 받는 컴포넌트.
/// </summary>
public sealed class FarmInput : MonoBehaviour
{
    private struct PointerData
    {
        public Vector2 PreviousScreenPosition;
        public Vector2 CurrentScreenPosition;
        public bool WasPreviousFrameInput;
    }

    private const float MaximumProjectionSize = 10.0f; // 가장 넓게 볼 때의 크기를 의미.
    private const float MinimumProjectionSize = 5.0f; // 가장 좁게 볼 때의 크기를 의미.

    private const float MovableWidth = 10.8f;
    private const float MovableHeight = 4.9f;

    [SerializeField] private InputActionReference tapAction;
    [SerializeField] private InputActionReference holdAction;
    [SerializeField] private InputActionReference mouseWheelAction;
    [SerializeField] private InputActionReference primaryPointerAction;
    [SerializeField] private InputActionReference secondaryPointerAction;

    private PointerData _primaryPointerData;
    private PointerData _secondaryPointerData;

    public float InputPriorityCut { get; set; } // 이 수치보다 작은 Priority를 가진 레이어는 입력 처리 대상이 아니도로 하는 프로퍼티.

    private Camera _camera;

    private IFarmInputLayer _holdingLayer;

    private Vector2 _initialHoldWorldPosition;
    private List<IFarmInputLayer> _inputLayers;
    private List<Func<bool>> _canInputConditions;
    private bool _canInput; // Update()에서 _canInputConditions에 의해 평가됨.

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

    public void AddCanInputCondition(Func<bool> condition) => _canInputConditions.Add(condition);

    private void FixedUpdate()
    {
        _primaryPointerData.PreviousScreenPosition = _primaryPointerData.CurrentScreenPosition;
        _secondaryPointerData.PreviousScreenPosition = _secondaryPointerData.CurrentScreenPosition;
        _primaryPointerData.CurrentScreenPosition = primaryPointerAction.action.ReadValue<Vector2>();
        _secondaryPointerData.CurrentScreenPosition = secondaryPointerAction.action.ReadValue<Vector2>();

        var isPrimaryPointerCurrentInput = primaryPointerAction.action.phase != InputActionPhase.Waiting;
        var isSecondaryPointerCurrentInput = secondaryPointerAction.action.phase != InputActionPhase.Waiting;
        if (!_primaryPointerData.WasPreviousFrameInput && isPrimaryPointerCurrentInput)
        {
            _primaryPointerData.PreviousScreenPosition = _primaryPointerData.CurrentScreenPosition;
        }
        if (!_secondaryPointerData.WasPreviousFrameInput && isSecondaryPointerCurrentInput)
        {
            _secondaryPointerData.PreviousScreenPosition = _secondaryPointerData.CurrentScreenPosition;
        }

        _primaryPointerData.WasPreviousFrameInput = isPrimaryPointerCurrentInput;
        _secondaryPointerData.WasPreviousFrameInput = isSecondaryPointerCurrentInput;
        
        _canInput = _canInputConditions.All(canInput => canInput());
        if (!_canInput)
        {
            return;
        }

        var currentPointerWorldPosition = GetCurrentPrimaryPointerWorldPosition();
        if (holdAction.action.phase == InputActionPhase.Started)
        {
            _initialHoldWorldPosition = currentPointerWorldPosition;
        }

        HandleHolding(currentPointerWorldPosition);
        HandleCameraMove();
        HandleZoom();
    }

    private void OnEnable()
    {
        tapAction.action.performed += OnTapActionPerformed;
    }

    private void OnDisable()
    {
        tapAction.action.performed -= OnTapActionPerformed;
    }

    private void Awake()
    {
        _canInputConditions = new();
        _inputLayers = new();
        _camera = GetComponent<Camera>();
        _camera.tag = "MainCamera";
    }

    private void MoveCamera(Vector2 worldPosition)
    {
        var newPosition = new Vector3(worldPosition.x, worldPosition.y, -10.0f);
        var movableMultiplier = (MaximumProjectionSize - _camera.orthographicSize) /
                                (MaximumProjectionSize - MinimumProjectionSize);

        newPosition.x = Mathf.Clamp(newPosition.x, -MovableWidth * movableMultiplier, MovableWidth * movableMultiplier);
        newPosition.y = Mathf.Clamp(newPosition.y, -MovableHeight * movableMultiplier,
            MovableHeight * movableMultiplier);
        transform.position = newPosition;
    }

    private void HandleHolding(Vector2 currentPointerWorldPosition)
    {
        var isHolding = holdAction.action.phase == InputActionPhase.Performed;
        if (!isHolding)
        {
            if (_holdingLayer is not null)
            {
                _holdingLayer.OnHold(_initialHoldWorldPosition, Vector2.zero, true, 0.0f);
                _holdingLayer = null;
            }
            return;
        }

        var deltaWorldPosition = new Vector2(currentPointerWorldPosition.x - _initialHoldWorldPosition.x,
            currentPointerWorldPosition.y - _initialHoldWorldPosition.y);

        if (_holdingLayer is not null)
        {
            _holdingLayer.OnHold(_initialHoldWorldPosition, deltaWorldPosition, false, Time.deltaTime);
            return;
        }

        foreach (var inputLayer in _inputLayers)
        {
            if (inputLayer.InputPriority < InputPriorityCut)
            {
                continue;
            }

            if (inputLayer.OnHold(
                    _initialHoldWorldPosition,
                    Vector2.zero,
                    false,
                    0.0f
                ))
            {
                _holdingLayer = inputLayer;
                break;
            }
        }
    }

    private void HandleCameraMove()
    {
        if (holdAction.action.phase != InputActionPhase.Performed || 
            _holdingLayer is not null)
        {
            return;
        }

        var previousTouchScreenPosition = _primaryPointerData.PreviousScreenPosition;
        var previousTouchWorldPosition = _camera.ScreenToWorldPoint(previousTouchScreenPosition);
        var currentTouchScreenPosition = _primaryPointerData.CurrentScreenPosition;
        var currentTouchWorldPosition = _camera.ScreenToWorldPoint(currentTouchScreenPosition);

        var deltaWorldPosition = currentTouchWorldPosition - previousTouchWorldPosition;
        if (secondaryPointerAction.action.phase != InputActionPhase.Waiting)
        {
            var previousSecondaryTouchScreenPosition = _secondaryPointerData.PreviousScreenPosition;
            var previousSecondaryTouchWorldPosition = _camera.ScreenToWorldPoint(previousSecondaryTouchScreenPosition);    
            var currentSecondaryTouchScreenPosition = _secondaryPointerData.CurrentScreenPosition;
            var currentSecondaryTouchWorldPosition = _camera.ScreenToWorldPoint(currentSecondaryTouchScreenPosition);

            deltaWorldPosition += currentSecondaryTouchWorldPosition - previousSecondaryTouchWorldPosition;
        }

        deltaWorldPosition *= -1.0f;

        var nextPosition = transform.position += deltaWorldPosition;
        nextPosition.x = Mathf.Clamp(nextPosition.x, -MovableWidth, MovableWidth);
        nextPosition.y = Mathf.Clamp(nextPosition.y, -MovableHeight, MovableWidth);
        
        MoveCamera(nextPosition);
    }

    private void HandleZoom()
    {
        var mouseWheelActionValue = mouseWheelAction.action.ReadValue<float>();
        if (mouseWheelActionValue != 0.0f)
        {
            _zoomMomentum = -5.0f * mouseWheelActionValue;
        }

        if (primaryPointerAction.action.phase != InputActionPhase.Waiting &&
            secondaryPointerAction.action.phase != InputActionPhase.Waiting)
        {
            var previousDelta = _primaryPointerData.PreviousScreenPosition - _secondaryPointerData.PreviousScreenPosition;
            var currentDelta = _primaryPointerData.CurrentScreenPosition - _secondaryPointerData.CurrentScreenPosition;

            var scale = currentDelta.sqrMagnitude != 0.0f ? previousDelta.magnitude / currentDelta.magnitude : 1.0f;
            var previousOrthographicSize = _camera.orthographicSize;
            _camera.orthographicSize = Mathf.Clamp(previousOrthographicSize * scale, MinimumProjectionSize, MaximumProjectionSize);
        }

        var currentProjectionSize = _camera.orthographicSize;
        var newProjectionSize = Mathf.Clamp(currentProjectionSize + _zoomMomentum * 0.07f, MinimumProjectionSize,
            MaximumProjectionSize);
        _camera.orthographicSize = newProjectionSize;


        var dampedMomentum = _zoomMomentum < 0.0f
            ? Mathf.Min(_zoomMomentum + Time.deltaTime * 5.0f, 0.0f)
            : Mathf.Max(_zoomMomentum - Time.deltaTime * 5.0f, 0.0f);
        _zoomMomentum = dampedMomentum;
        MoveCamera(new Vector2(transform.position.x, transform.position.y));
    }

    private void OnTapActionPerformed(InputAction.CallbackContext context)
    {
        foreach (var inputLayer in _inputLayers)
        {
            if (inputLayer.OnTap(GetCurrentPrimaryPointerWorldPosition()))
            {
                break;
            }
        }
    }

    private Vector2 GetCurrentPrimaryPointerWorldPosition() =>
        _camera.ScreenToWorldPoint(_primaryPointerData.CurrentScreenPosition);
}
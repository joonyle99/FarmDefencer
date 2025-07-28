using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using System.Linq;
using Sirenix.OdinInspector;
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

    [InfoBox("가장 좁게, 가까이서 볼 때의 크기를 의미, 기기 화면이 담을 유니티 좌표계 Y 크기를 의미.")] [SerializeField]
    private float minimumProjectionSize = 5.0f;

    [Header("카메라 이동 가능 맵 경계")] [SerializeField]
    private Rect mapBounds = new(0.0f, 0.0f, 21.0f, 11.0f);

    [SerializeField] private InputActionReference tapAction;
    [SerializeField] private InputActionReference mouseWheelAction;
    [SerializeField] private InputActionReference holdAction;
    [SerializeField] private InputActionReference primaryPointerAction;
    [SerializeField] private InputActionReference secondaryPointerAction;

    private PointerData _primaryPointerData;
    private PointerData _secondaryPointerData;
    private float _elapsedHoldTime;

    public float InputPriorityCut { get; set; } // 이 수치보다 작은 Priority를 가진 레이어는 입력 처리 대상이 아니도로 하는 프로퍼티.

    public Camera Camera { get; private set; }

    private IFarmInputLayer _holdingLayer;

    private Vector2 _initialHoldWorldPosition;
    private List<IFarmInputLayer> _inputLayers;
    private List<Func<bool>> _canInputConditions;
    private bool _canInput; // Update()에서 _canInputConditions에 의해 평가됨.

    private float _zoomMomentum;

    public void FullZoomOut()
    {
        Camera.orthographicSize = GetMaximumProjectionSize();
    }

    public void RegisterInputLayer(IFarmInputLayer inputLayer)
    {
        _inputLayers.Add(inputLayer);
        _inputLayers.Sort((left, right) => right.InputPriority.CompareTo(left.InputPriority));
    }

    public void AddCanInputCondition(Func<bool> condition) => _canInputConditions.Add(condition);

    private void FixedUpdate()
    {
        if (primaryPointerAction.action.phase != InputActionPhase.Waiting)
        {
            _elapsedHoldTime += Time.fixedDeltaTime;
        }
        else
        {
            _elapsedHoldTime = 0.0f;
        }

        _primaryPointerData.PreviousScreenPosition = _primaryPointerData.CurrentScreenPosition;
        _secondaryPointerData.PreviousScreenPosition = _secondaryPointerData.CurrentScreenPosition;
        _primaryPointerData.CurrentScreenPosition = primaryPointerAction.action.ReadValue<Vector2>();
        _secondaryPointerData.CurrentScreenPosition = secondaryPointerAction.action.ReadValue<Vector2>();

        var isPrimaryPointerCurrentInput = primaryPointerAction.action.phase != InputActionPhase.Waiting;
        var isSecondaryPointerCurrentInput = secondaryPointerAction.action.phase != InputActionPhase.Waiting;
        var currentPointerWorldPosition = GetCurrentPrimaryPointerWorldPosition();
        if (!_primaryPointerData.WasPreviousFrameInput && isPrimaryPointerCurrentInput)
        {
#if !UNITY_EDITOR
            _initialHoldWorldPosition = currentPointerWorldPosition;
#endif
            _primaryPointerData.PreviousScreenPosition = _primaryPointerData.CurrentScreenPosition;
        }

        if (!_secondaryPointerData.WasPreviousFrameInput && isSecondaryPointerCurrentInput)
        {
            _secondaryPointerData.PreviousScreenPosition = _secondaryPointerData.CurrentScreenPosition;
        }

        _primaryPointerData.WasPreviousFrameInput = isPrimaryPointerCurrentInput;
        _secondaryPointerData.WasPreviousFrameInput = isSecondaryPointerCurrentInput;

#if UNITY_EDITOR
        if (holdAction.action.phase == InputActionPhase.Started)
        {
            _initialHoldWorldPosition = currentPointerWorldPosition;
        }
#endif
        _canInput = _canInputConditions.All(canInput => canInput());
        if (!_canInput)
        {
            return;
        }

        HandleHolding(currentPointerWorldPosition);
        HandleCameraMove();
        HandleZoom();
    }

    private void LateUpdate()
    {
        Camera.orthographicSize =
            Mathf.Clamp(Camera.orthographicSize, minimumProjectionSize, GetMaximumProjectionSize());
        var cameraPosition = transform.position;
        var newPosition = cameraPosition;

        var verticalExtent = Camera.orthographicSize;
        var horizontalExtent = verticalExtent * Camera.aspect;

        newPosition.y = Mathf.Clamp(newPosition.y, mapBounds.yMin + verticalExtent, mapBounds.yMax - verticalExtent);
        newPosition.x = Mathf.Clamp(newPosition.x, mapBounds.xMin + horizontalExtent,
            mapBounds.xMax - horizontalExtent);
        newPosition.z = -10.0f;

        transform.position = newPosition;
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
        Camera = GetComponent<Camera>();
        Camera.tag = "MainCamera";
    }

    private void HandleHolding(Vector2 currentPointerWorldPosition)
    {
        var isHolding =
#if UNITY_EDITOR
            holdAction.action.phase == InputActionPhase.Performed;
#else
            primaryPointerAction.action.phase == InputActionPhase.Started || primaryPointerAction.action.phase == InputActionPhase.Performed;
#endif
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
        var primaryPointerActionPhase = primaryPointerAction.action.phase;
        var secondaryPointerActionPhase = secondaryPointerAction.action.phase;
        
        if (
#if UNITY_EDITOR
            holdAction.action.phase != InputActionPhase.Performed ||
#endif
            _elapsedHoldTime < 0.1f || 
            _holdingLayer is not null ||
            primaryPointerActionPhase != InputActionPhase.Started && secondaryPointerActionPhase == InputActionPhase.Waiting)
        {
            return;
        }

        var previousPrimaryTouchScreenPosition = _primaryPointerData.PreviousScreenPosition;
        var currentPrimaryTouchScreenPosition = _primaryPointerData.CurrentScreenPosition;
        
        var previousSecondaryTouchScreenPosition = _secondaryPointerData.PreviousScreenPosition;
        var currentSecondaryTouchScreenPosition = _secondaryPointerData.CurrentScreenPosition;
        
        var previousPrimaryTouchWorldPosition = Camera.ScreenToWorldPoint(previousPrimaryTouchScreenPosition);
        var currentPrimaryTouchWorldPosition = Camera.ScreenToWorldPoint(currentPrimaryTouchScreenPosition);
        var previousSecondaryTouchWorldPosition = Camera.ScreenToWorldPoint(previousSecondaryTouchScreenPosition);
        var currentSecondaryTouchWorldPosition = Camera.ScreenToWorldPoint(currentSecondaryTouchScreenPosition);

        Vector3 deltaWorldPosition;
        if (primaryPointerActionPhase != InputActionPhase.Waiting && secondaryPointerActionPhase != InputActionPhase.Waiting)
        {
            deltaWorldPosition = (currentPrimaryTouchWorldPosition - previousPrimaryTouchWorldPosition +
                                 currentSecondaryTouchWorldPosition - previousSecondaryTouchWorldPosition) / 2.0f;
        }
        else if (primaryPointerActionPhase != InputActionPhase.Waiting)
        {
            deltaWorldPosition = currentPrimaryTouchWorldPosition - previousPrimaryTouchWorldPosition;
        }
        else
        {
             deltaWorldPosition = currentSecondaryTouchWorldPosition - previousSecondaryTouchWorldPosition;
        }

        deltaWorldPosition *= -1.0f;

        var nextPosition = transform.position += deltaWorldPosition;
        transform.position = nextPosition;
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
            var previousDelta = _primaryPointerData.PreviousScreenPosition -
                                _secondaryPointerData.PreviousScreenPosition;
            var currentDelta = _primaryPointerData.CurrentScreenPosition - _secondaryPointerData.CurrentScreenPosition;

            var scale = currentDelta.sqrMagnitude != 0.0f ? previousDelta.magnitude / currentDelta.magnitude : 1.0f;
            var previousOrthographicSize = Camera.orthographicSize;
            Camera.orthographicSize = previousOrthographicSize * scale;
        }

        var currentProjectionSize = Camera.orthographicSize;
        Camera.orthographicSize = currentProjectionSize + _zoomMomentum * 0.07f;


        var dampedMomentum = _zoomMomentum < 0.0f
            ? Mathf.Min(_zoomMomentum + Time.deltaTime * 5.0f, 0.0f)
            : Mathf.Max(_zoomMomentum - Time.deltaTime * 5.0f, 0.0f);
        _zoomMomentum = dampedMomentum;
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
        Camera.ScreenToWorldPoint(_primaryPointerData.CurrentScreenPosition);

    private float GetMaximumProjectionSize() => Mathf.Min(mapBounds.height / 2, mapBounds.width / 2 / Camera.aspect);
}
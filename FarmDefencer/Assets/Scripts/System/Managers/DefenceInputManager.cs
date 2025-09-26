using UnityEngine;

public class DefenceInputManager : JoonyleGameDevKit.Singleton<DefenceInputManager>
{
    private DefenceInput _defenceInput;

    private Vector2 _pointerPosition;
    public Vector2 PointerPosition => _pointerPosition;

    private Vector2 _pointerDelta;
    public Vector2 PointerDelta => _pointerDelta;

    private float _mouseScrollY;
    public float MouseScrollY => _mouseScrollY;

    public bool OnPointerPressed => _defenceInput.DefenceActionMap.PointerPress.IsPressed();
    public bool OnPointerPressedThisFrame => _defenceInput.DefenceActionMap.PointerPress.WasPressedThisFrame();
    public bool OnPointerReleaseThisFrame => _defenceInput.DefenceActionMap.PointerPress.WasReleasedThisFrame();

    protected override void Awake()
    {
        base.Awake();

        _defenceInput = new DefenceInput();
    }
    private void OnEnable()
    {
        _defenceInput.Enable(); // 하위 Action들 모두 Enable
    }
    private void OnDisable()
    {
        _defenceInput.Disable(); // 하위 Action들 모두 Disable
    }
    private void Update()
    {
        _pointerPosition = _defenceInput.DefenceActionMap.PointerPosition.ReadValue<Vector2>();
        _pointerDelta = _defenceInput.DefenceActionMap.PointerDelta.ReadValue<Vector2>();
        _mouseScrollY = _defenceInput.DefenceActionMap.MouseScrollY.ReadValue<float>();
    }
}

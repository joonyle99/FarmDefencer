using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private CameraContentFitter _contentFitter;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _zoomSpeed = 50f;

    public float CameraWidth => CameraHeight * CameraAspect;
    public float CameraHeight => _camera.orthographicSize * 2f;
    public float CameraAspect => (float)Screen.width / (float)Screen.height;

    public Vector3 CameraPos => _camera.transform.position;
    public Vector3 CameraMinPoint => new Vector3(CameraPos.x - CameraWidth * 0.5f, CameraPos.y - CameraHeight * 0.5f, CameraPos.z);
    public Vector3 CameraMaxPoint => new Vector3(CameraPos.x + CameraWidth * 0.5f, CameraPos.y + CameraHeight * 0.5f, CameraPos.z);

    private bool _isDrag = false;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _contentFitter = GetComponent<CameraContentFitter>();
    }
    private void Start()
    {
        var gridMap = DefenceContext.Current?.GridMap;

        if (gridMap == null)
        {
            return;
        }

        // 카메라 초기 위치 설정
        var cameraPos = CameraPos;
        cameraPos.x = gridMap.CenterWorldPos.x;
        cameraPos.y = gridMap.CenterWorldPos.y;
        _camera.transform.position = cameraPos;
    }
    private void LateUpdate()
    {
        HandleDragInput();
        HandleCameraZoom();
        HandleCameraMove();
    }

    private void HandleDragInput()
    {
        var defenceInput = DefenceInputManager.Instance;

        if (defenceInput.OnPointerPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            _isDrag = true;
        }
        else if (defenceInput.OnPointerReleaseThisFrame)
        {
            _isDrag = false;
        }
    }
    private void HandleCameraZoom()
    {
        var defenceInput = DefenceInputManager.Instance;

        var mouseScrollY = defenceInput.MouseScrollY;

        if (Mathf.Abs(mouseScrollY) > Mathf.Epsilon)
        {
            _camera.orthographicSize -= mouseScrollY * _zoomSpeed * Time.deltaTime;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _contentFitter.MinOrthograpicSize, _contentFitter.MaxOrthograpicSize);
        }
    }
    private void HandleCameraMove()
    {
        var defenceInput = DefenceInputManager.Instance;

        var cameraPos = CameraPos;

        if (_isDrag && defenceInput.OnPointerPressed)
        {
            var pointerDelta = defenceInput.PointerDelta;

            if (pointerDelta != Vector2.zero)
            {
                cameraPos.x -= pointerDelta.x * _moveSpeed * Time.deltaTime;
                cameraPos.y -= pointerDelta.y * _moveSpeed * Time.deltaTime;
            }
        }

        SetCameraPos(cameraPos);
    }

    private void SetCameraPos(Vector3 cameraPos)
    {
        // GOGO: Y축에 대해서도 해야한다
        // 무한정 이동할 수 는 없도록 해야함

        var contentMinPoint = _contentFitter.ContentMinPoint;
        var contentMaxPoint = _contentFitter.ContentMaxPoint;
        var minPosX = contentMinPoint.x + CameraWidth * 0.5f;
        var maxPosX = contentMaxPoint.x - CameraWidth * 0.5f;
        cameraPos.x = Mathf.Clamp(cameraPos.x, minPosX, maxPosX);
        _camera.transform.position = cameraPos;
    }
}

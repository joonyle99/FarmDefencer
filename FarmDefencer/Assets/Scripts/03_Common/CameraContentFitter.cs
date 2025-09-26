using UnityEngine;

/// <summary>
/// 직교 카메라의 사이즈를 콘텐츠의 가로 길이에 딱 맞도록 한다
/// 어떤 해상도이든 콘텐츠가 모두 보여야 할 때 사용한다
/// </summary>
public class CameraContentFitter : MonoBehaviour
{
    [SerializeField] private MeshRenderer _contentRenderer;

    public Vector3 ContentSize => _contentRenderer.bounds.size;
    public float ContentWidth => _contentRenderer.bounds.size.x;
    public float ContentHeight => _contentRenderer.bounds.size.y;
    public Vector3 ContentMinPoint => _contentRenderer.bounds.min;
    public Vector3 ContentMaxPoint => _contentRenderer.bounds.max;

    [SerializeField] private float _minOrthograpicSize = 3f;
    public float MinOrthograpicSize => _minOrthograpicSize;
    private float _maxOrthograpicSize;
    public float MaxOrthograpicSize => _maxOrthograpicSize;

    private Camera _camera;
    private CameraController _cameraController;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _cameraController = GetComponent<CameraController>();
    }
    private void Start()
    {
        // 아래 공식을 활용
        // 1. 화면 높이 = orthograpicSize * 2
        // 2. 화면 너비 = 화면 높이 * aspect

        var orthograpicSize = (ContentWidth / _cameraController.CameraAspect) * 0.5f;
        _camera.orthographicSize = orthograpicSize;
        _maxOrthograpicSize = orthograpicSize;
    }
}

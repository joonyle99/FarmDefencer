using UnityEngine;

/// <summary>
/// 직교 카메라의 사이즈를 콘텐츠의 가로 길이에 딱 맞도록 한다
/// 어떤 해상도이든 콘텐츠가 모두 보여야 할 때 사용한다
/// </summary>
public class OrthograpicCameraFitter : MonoBehaviour
{
    [SerializeField] private MeshRenderer _contentRenderer;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void Start()
    {
        // 아래 공식을 활용
        // 1. 화면 높이 = orthograpicSize * 2
        // 2. 화면 너비 = 화면 높이 * aspect

        var contentWidth = _contentRenderer.bounds.size.x;
        var currAspect = (float)Screen.width / (float)Screen.height;
        var orthograpicSize = (contentWidth / currAspect) / 2f;
        _camera.orthographicSize = orthograpicSize;
    }
}

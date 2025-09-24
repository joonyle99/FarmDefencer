using UnityEngine;

public class DefenceCameraController : MonoBehaviour
{
    [SerializeField] private MeshRenderer _backgroundRenderer;
    private Camera _defeceCamera;

    private void Awake()
    {
        _defeceCamera = GetComponent<Camera>();
    }
    private void Start()
    {
        var gridMap = DefenceContext.Current.GridMap;

        if (gridMap == null)
        {
            return;
        }

        var cameraPos = _defeceCamera.transform.position;
        cameraPos.x = gridMap.CenterWorldPos.x;
        cameraPos.y = gridMap.CenterWorldPos.y;
        _defeceCamera.transform.position = cameraPos;

        // 내가 하고싶은 것
        // -> 2340 x 1080 표준 해상도 딱 맞게 설정된
        // -> 월드 스페이스에 존재하는 Background Video(video player 컴포넌트와 quad + material 사용)와
        // -> Tilemap이 해상도에 따라 변경되는 스크린 사이즈에 딱 맞게 들어오게 하도록 하기 위해서
        // -> 카메라 사이즈를 딱 맞게 변경해야 한다

        // background의 bounds를 구한다
        Bounds backgroundBounds = _backgroundRenderer.bounds;
        // screen aspect를 구한다
        float aspect = (float)Screen.width / (float)Screen.height;
    }
}

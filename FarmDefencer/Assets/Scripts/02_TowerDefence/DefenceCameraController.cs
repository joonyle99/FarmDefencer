using UnityEngine;

public class DefenceCameraController : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void Start()
    {
        var gridMap = DefenceContext.Current.GridMap;

        if (gridMap == null)
        {
            return;
        }

        var cameraPos = _camera.transform.position;
        cameraPos.x = gridMap.CenterWorldPos.x;
        cameraPos.y = gridMap.CenterWorldPos.y;
        _camera.transform.position = cameraPos;
    }
}

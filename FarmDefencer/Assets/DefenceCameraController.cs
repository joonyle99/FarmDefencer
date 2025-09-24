using UnityEngine;

public class DefenceCameraController : MonoBehaviour
{
    private Camera _defeceCamera;
    public Camera DefenceCamera => _defeceCamera;

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
    }
}

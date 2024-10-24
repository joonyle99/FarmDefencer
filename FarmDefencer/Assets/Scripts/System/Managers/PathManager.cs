using UnityEngine;

public class PathManager : JoonyleGameDevKit.Singleton<PathManager>
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform[] _path;

    public Transform[] Path => _path;
}

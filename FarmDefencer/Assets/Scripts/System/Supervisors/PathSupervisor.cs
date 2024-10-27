using UnityEngine;

/// <summary>
/// 타워 디펜스에서 등장하는 몬스터가 따라가는 경로를 관리합니다.
/// </summary>
/// <remarks>
/// Manager vs Supervisor에 대한 설명은 <see cref="ManagerVsSupervisor"/>를 참조하세요
/// </remarks>
public class PathSupervisor : JoonyleGameDevKit.Singleton<PathSupervisor>
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform[] _path;

    public Transform StartPoint => _startPoint;
    public Transform[] Path => _path;
}
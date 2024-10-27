using UnityEngine;

/// <summary>
/// Ÿ�� ���潺���� �����ϴ� ���Ͱ� ���󰡴� ��θ� �����մϴ�.
/// </summary>
/// <remarks>
/// Manager vs Supervisor�� ���� ������ <see cref="ManagerVsSupervisor"/>�� �����ϼ���
/// </remarks>
public class PathSupervisor : JoonyleGameDevKit.Singleton<PathSupervisor>
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform[] _path;

    public Transform StartPoint => _startPoint;
    public Transform[] Path => _path;
}
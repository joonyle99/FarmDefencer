using UnityEngine;

/// <summary>
/// Ÿ�� ���潺���� �����ϴ� ���Ͱ� ���󰡴� ��θ� �����մϴ�.
/// </summary>
/// <remarks>
/// Manager vs Supervisor �� ���� ������ <see cref="ManagerClassGuideline"/>�� �����ϼ���
/// </remarks>
public class PathSupervisor : JoonyleGameDevKit.Singleton<PathSupervisor>
{
    [SerializeField] private Pathway[] _pathways;

    private int _pathwayIndex = 0;

    public Pathway GetRandomPathway()
    {
        _pathwayIndex += Random.Range(0, _pathways.Length);
        _pathwayIndex %= _pathways.Length;

        return _pathways[_pathwayIndex];
    }
}
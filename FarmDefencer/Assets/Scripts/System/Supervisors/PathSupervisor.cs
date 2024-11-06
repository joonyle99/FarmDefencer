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
    public int PathwayIndex
    {
        get => _pathwayIndex;
        set
        {
            _pathwayIndex = value;

            if (_pathwayIndex >= _pathways.Length)
            {
                _pathwayIndex = 0;
            }
        }
    }

    public Pathway GetCurrentPathway()
    {
        return _pathways[PathwayIndex++];
    }
}
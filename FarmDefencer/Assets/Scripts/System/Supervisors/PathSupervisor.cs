using UnityEngine;

/// <summary>
/// 타워 디펜스에서 등장하는 몬스터가 따라가는 경로를 관리합니다.
/// </summary>
/// <remarks>
/// Manager vs Supervisor 에 대한 설명은 <see cref="ManagerClassGuideline"/>를 참조하세요
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
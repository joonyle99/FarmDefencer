using UnityEngine;

/// <summary>
/// 맵 식별에 사용되며 부가 정보를 담을 수 있는 엔트리.
/// </summary>
[CreateAssetMenu(fileName = "MapEntry", menuName = "Scriptable Objects/MapEntry")]
public sealed class MapEntry : ScriptableObject
{
    [SerializeField] private int mapId; // 1 ~ N
    [SerializeField] private string mapCode;

    public int MapId => mapId;
    public string MapCode => mapCode;

    [SerializeField] private Monster[] monsters;
    public Monster[] Monsters => monsters;

    [SerializeField] private string[] crops;
    public string[] Crops => crops;
}

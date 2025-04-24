using UnityEngine;

public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>
{
    [SerializeField] private MapEntry[] mapEntries;
    
    public MapEntry CurrentMap => mapEntries[0]; // 추후 실제 현재 맵 저장 및 반환 로직 구현 필요
}

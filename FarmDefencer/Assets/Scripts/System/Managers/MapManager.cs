using UnityEngine;

/// <summary>
/// </summary>
public sealed class MapManager : JoonyleGameDevKit.Singleton<MapManager>
{
    public const int Map_Grassland = 1;
    public const int Map_Beach = 2;
    public const int Map_Cave = 3;
    
    public int CurrentMap => Map_Grassland;
}

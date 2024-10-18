using UnityEngine;

/// <summary>
/// 게임 시작 시 필요한 매니저 및 시스템을 초기화합니다
/// </summary>
public class Bootstrapper : JoonyleGameDevKit.PersistentSingleton<Bootstrapper>
{
    public void InitializeGame()
    {
        Debug.Log("Initialize Game");
    }
}

using UnityEngine;

/// <summary>
/// ���� ���� �� �ʿ��� �Ŵ��� �� �ý����� �ʱ�ȭ�մϴ�
/// </summary>
public class Bootstrapper : JoonyleGameDevKit.PersistentSingleton<Bootstrapper>
{
    public void InitializeGame()
    {
        Debug.Log("Initialize Game");
    }
}
